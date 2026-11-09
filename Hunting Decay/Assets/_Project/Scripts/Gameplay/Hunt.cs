using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace HuntingDecay
{
    /// <summary>
    /// The forest part of the day: moving between the photos, finding what's under the scope,
    /// and taking the one shot.
    /// </summary>
    public class Hunt : MonoBehaviour
    {
        [SerializeField] Scope scope;
        [SerializeField] ScopeHud hud;
        [SerializeField] ViewManager viewManager;
        [Tooltip("The photos the arrows cycle through, in order. The hunt starts on the first one.")]
        [SerializeField] List<View> huntViews = new();
        [SerializeField] GameObject arrows;
        [SerializeField] Button leftArrow;
        [SerializeField] Button rightArrow;
        [SerializeField] InputAction shootAction = new("Shoot", InputActionType.Button, "<Mouse>/leftButton");

        [Header("Sounds")]
        [Tooltip("Raising the rifle, when aiming starts. The breathing loop is on the AudioManager.")]
        [SerializeField] Sound aimSound;
        [SerializeField] Sound gunshotSound;

        readonly List<Collider2D> overlaps = new();
        // Anomalies revealed during the current zoom. They swap back when the zoom ends.
        readonly List<ZoomReveal> revealed = new();
        // Sound cues of the open photo, like the cave voice. They only run until the shot.
        readonly List<AmbientCue> cues = new();
        // Set once a photo's HuntMusic replaces the day's music, so the shot can stop it.
        bool huntMusicPlaying;
        ContactFilter2D targetFilter;
        int day;
        bool isFinalDay;
        StoryData story;
        int viewIndex;
        Target current;

        /// <summary>The target hit by the day's shot, once it's taken.</summary>
        public Target Shot { get; private set; }

        /// <summary>Whether today's photos hold an anomaly, which decides if shooting anything else lets it escape.</summary>
        public bool AnomalyPresent => huntViews
            .SelectMany(view => view.GetComponentsInChildren<Target>(true))
            .Any(target => target.gameObject.activeSelf && target.Type == TargetType.Anomaly);

        void Awake()
        {
            targetFilter = ContactFilter2D.noFilter;
            leftArrow.onClick.AddListener(() => Step(-1));
            rightArrow.onClick.AddListener(() => Step(1));
            arrows.SetActive(false);
        }

        void OnEnable() => shootAction.Enable();

        void OnDisable() => shootAction.Disable();

        /// <summary>Shows the first photo and hands control to the player.</summary>
        public void Begin(int day, StoryData story)
        {
            this.day = day;
            this.story = story;
            isFinalDay = day == story.days.Count;
            Shot = null;
            current = null;
            viewIndex = 0;
            ShowView(huntViews[viewIndex]);
            hud.ShowNote(story.days[day - 1].scopeNote);
            hud.ShowTarget(null, false, null);
            scope.AllowAiming = true;
        }

        /// <summary>Runs until the shot is taken. Read <see cref="Shot"/> afterwards.</summary>
        public IEnumerator WaitForShot()
        {
            bool tutorial = day == ShotRules.TutorialDay;
            bool hasAimed = false;
            if (tutorial)
                hud.ShowHint(story.aimHint);

            bool wasAiming = false;
            while (Shot == null)
            {
                arrows.SetActive(!scope.IsAiming);
                if (scope.IsAiming && !wasAiming)
                    AudioManager.Instance.Play(aimSound);
                if (!scope.IsAiming && wasAiming)
                    HideRevealed();
                wasAiming = scope.IsAiming;
                AudioManager.Instance.SetBreathing(scope.IsAiming);
                foreach (AmbientCue cue in cues)
                    cue.Tick(Time.deltaTime);

                Target target = scope.IsFullyZoomed ? FindTarget(scope.AimPoint) : null;
                bool canShoot = target != null && ShotRules.CanShoot(target.Type, day, isFinalDay);
                if (target != current)
                {
                    current = target;
                    if (current != null && current.TryGetComponent(out ZoomReveal reveal) && !reveal.IsRevealed)
                    {
                        reveal.Reveal();
                        revealed.Add(reveal);
                    }
                    if (current != null && current.TryGetComponent(out ScopeSound scopeSound))
                        scopeSound.Play();
                    hud.ShowTarget(current, canShoot, story.shootPrompt);
                }

                if (tutorial && !hasAimed && scope.IsAiming)
                {
                    hasAimed = true;
                    hud.HideHint();
                }

                if (canShoot && ShootPressed())
                {
                    Shot = current;
                    StopCues();
                    // A jumpscare target never takes the shot, so there's no gunshot either.
                    if (!current.Jumpscare.enabled)
                        AudioManager.Instance.Play(gunshotSound);
                }
                yield return null;
            }
            arrows.SetActive(false);
            hud.HideHint();
        }

        /// <summary>Leaves the scope and hides the hunt interface.</summary>
        public void Stop()
        {
            scope.AllowAiming = false;
            arrows.SetActive(false);
            hud.ShowTarget(null, false, null);
            hud.HideHint();
            AudioManager.Instance.SetBreathing(false);
            HideRevealed();
            StopCues();
            current = null;
            AudioManager.Instance.PlayPhotoAmbience(null);
            if (huntMusicPlaying)
            {
                AudioManager.Instance.PlayMusic(null);
                huntMusicPlaying = false;
            }
        }

        void HideRevealed()
        {
            foreach (ZoomReveal reveal in revealed)
                reveal.Hide();
            revealed.Clear();
        }

        void Step(int direction)
        {
            viewIndex = (viewIndex + direction + huntViews.Count) % huntViews.Count;
            ShowView(huntViews[viewIndex]);
        }

        void ShowView(View view)
        {
            StopCues();
            viewManager.Show(view);
            AudioManager.Instance.PlayPhotoAmbience(view.Ambience);
            // Only today's cues: DayVisibility has already turned the others off.
            cues.AddRange(view.GetComponentsInChildren<AmbientCue>());
            foreach (AmbientCue cue in cues)
                cue.Begin();
            // Only today's music too. Once started it keeps playing in every photo until the shot.
            foreach (HuntMusic music in view.GetComponentsInChildren<HuntMusic>())
            {
                AudioManager.Instance.PlayMusic(music.Sound);
                huntMusicPlaying = true;
            }
        }

        void StopCues()
        {
            foreach (AmbientCue cue in cues)
                cue.Stop();
            cues.Clear();
        }

        bool ShootPressed() =>
            !PauseMenu.IsPaused
            && shootAction.WasPressedThisFrame()
            && !EventSystem.current.IsPointerOverGameObject();

        /// <summary>The front-most target under the aim point, found with Physics2D.OverlapPoint.</summary>
        Target FindTarget(Vector2 point)
        {
            Physics2D.OverlapPoint(point, targetFilter, overlaps);
            Target best = null;
            foreach (Collider2D overlap in overlaps)
            {
                if (overlap.TryGetComponent(out Target target) && (best == null || target.SortingOrder > best.SortingOrder))
                    best = target;
            }
            return best;
        }
    }
}
