using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HuntingDecay
{
    /// <summary>
    /// The game loop from the GDD, written top to bottom: day card, breakfast, hunt, shot, dinner,
    /// next day, until an ending.
    /// </summary>
    public class GameFlow : MonoBehaviour
    {
        static readonly List<DialogueLine> NoLines = new();

        [SerializeField] StoryData story;
        [SerializeField] GameState state = new();

        [Header("Views")]
        [SerializeField] ViewManager viewManager;
        [SerializeField] View titleView;
        [SerializeField] HouseView houseView;
        [SerializeField] EndingView endingView;

        [Header("Systems")]
        [SerializeField] Hunt hunt;
        [SerializeField] Atmosphere atmosphere;
        [SerializeField] DialogueBox dialogue;
        [SerializeField] ScreenFader fader;
        [SerializeField] JumpscareScreen jumpscare;
        [SerializeField] TitleScreen title;
        [SerializeField] PauseMenu pauseMenu;

        [Header("Timing (seconds)")]
        [SerializeField, Min(0f)] float fadeDuration = 0.8f;
        [SerializeField, Min(0f)] float dayCardHold = 1.2f;
        [Tooltip("How long the shot's white flash takes to fade into black.")]
        [SerializeField, Min(0f)] float flashToBlackDuration = 1f;
        [Tooltip("The short flash before a jumpscare.")]
        [SerializeField, Min(0f)] float flashDuration = 0.06f;
        [SerializeField, Min(0f)] float jumpscareHold = 0.9f;

        public GameState State => state;

        void Awake()
        {
            title.NewGameClicked += () => StartRun(1, 0);
            pauseMenu.QuitToTitleClicked += ShowTitle;
        }

        void Start() => ShowTitle();

        public void ShowTitle()
        {
            StopAllCoroutines();
            HideGameInterface();
            fader.SetBlack(true);
            state.Set(1, 0);
            ApplyState();
            atmosphere.ApplyNeutral();
            viewManager.Show(titleView);
            title.Show();
            pauseMenu.CanPause = false;
            StartCoroutine(fader.FadeFromBlack(fadeDuration));
        }

        /// <summary>Starts a run at any day. New Game uses day 1, and the dev shortcuts use the rest.</summary>
        public void StartRun(int day, int missedAnomalies)
        {
            StopAllCoroutines();
            pauseMenu.CanPause = true;
            state.Set(day, missedAnomalies);
            StartCoroutine(RunDays());
        }

        public void SetMissedAnomalies(int missedAnomalies)
        {
            state.Set(state.Day, missedAnomalies);
            ApplyState();
        }

        IEnumerator RunDays()
        {
            yield return fader.FadeToBlack(fadeDuration);
            title.Hide();
            HideGameInterface();

            while (true)
            {
                DayData day = story.days[state.Day - 1];
                ApplyState();
                atmosphere.ApplyDay(day);
                yield return fader.TypeCard(string.Format(story.dayCardFormat, state.Day));
                yield return new WaitForSeconds(dayCardHold);
                fader.ClearCard();

                if (day.hasBreakfast)
                    yield return PlayBreakfast(day);

                yield return PlayHunt(day);
                Target hit = hunt.Shot;
                bool isFinalDay = state.Day == story.days.Count;
                ShotResult result = ShotRules.Resolve(hit.Type, isFinalDay, hunt.AnomalyPresent);
                yield return PlayShot(hit);

                if (result.BloomAdvances)
                {
                    state.AddMissedAnomaly();
                    ApplyState();
                }

                if (result.Ending != Ending.None)
                {
                    yield return PlayEnding(result.Ending);
                    yield break;
                }

                if (state.MissedAnomalies >= GameState.MissedAnomaliesForEnding)
                {
                    yield return PlayDinner(story.lastDinner, NoLines);
                    yield return PlayEnding(Ending.EndOfHollowPine);
                    yield break;
                }

                Dinner dinner = state.MissedAnomalies == 0 ? day.dinnerHealthy : day.dinnerInfected;
                yield return PlayDinner(dinner, result.BloomAdvances ? day.missedNews : NoLines);
                state.NextDay();
            }
        }

        IEnumerator PlayBreakfast(DayData day)
        {
            houseView.SetScene(HouseScene.Breakfast);
            viewManager.Show(houseView);
            yield return fader.FadeFromBlack(fadeDuration);

            Breakfast breakfast = state.MissedAnomalies == 0 ? day.breakfastHealthy : day.breakfastInfected;
            yield return dialogue.Play(breakfast.greeting.Concat(breakfast.villageNews).Concat(day.huntRequest));
            dialogue.Hide();
            yield return fader.FadeToBlack(fadeDuration);
        }

        IEnumerator PlayHunt(DayData day)
        {
            atmosphere.EnterForest(day);
            hunt.Begin(state.Day, story);
            // The player can already look around while the photo fades in.
            StartCoroutine(fader.FadeFromBlack(fadeDuration));
            yield return hunt.WaitForShot();
        }

        IEnumerator PlayShot(Target hit)
        {
            if (hit.Jumpscare.enabled)
            {
                // The only exception to the fade: the face jumps on screen and cuts straight to dinner.
                yield return fader.Flash(flashDuration);
                yield return jumpscare.Play(hit.Jumpscare, jumpscareHold);
                fader.SetBlack(true);
                jumpscare.Hide();
            }
            else
            {
                yield return fader.FlashToBlack(flashToBlackDuration);
            }
            hunt.Stop();
            atmosphere.LeaveForest();
        }

        IEnumerator PlayDinner(Dinner dinner, List<DialogueLine> news)
        {
            houseView.SetScene(HouseScene.Dinner);
            viewManager.Show(houseView);
            yield return fader.FadeFromBlack(fadeDuration);

            yield return dialogue.Play(dinner.question);
            int choice = 0;
            yield return dialogue.Choose(dinner.replies.Select(reply => reply.text).ToList(), index => choice = index);
            yield return dialogue.Play(dinner.replies[choice].reaction.Concat(news));
            dialogue.Hide();
            yield return fader.FadeToBlack(fadeDuration);
        }

        IEnumerator PlayEnding(Ending ending)
        {
            List<DialogueLine> lines;
            switch (ending)
            {
                case Ending.HappyWinter:
                    endingView.ShowHappyWinter();
                    viewManager.Show(endingView);
                    lines = state.MissedAnomalies == 0 ? story.happyWinter : story.happyWinterWithLoss;
                    break;
                case Ending.Withering:
                    endingView.ShowWithering();
                    viewManager.Show(endingView);
                    lines = story.withering;
                    break;
                default:
                    houseView.SetScene(HouseScene.Overgrown);
                    viewManager.Show(houseView);
                    lines = story.endOfHollowPine;
                    break;
            }

            yield return fader.FadeFromBlack(fadeDuration);
            yield return dialogue.Play(lines);
            dialogue.Hide();
            yield return fader.FadeToBlack(fadeDuration);
            yield return fader.TypeCard(story.theEnd);
            yield return dialogue.WaitForClick();
            fader.ClearCard();
            ShowTitle();
        }

        void ApplyState()
        {
            viewManager.ApplyState(state.Day, state.MissedAnomalies);
            AudioManager.Instance.SetBloom(state.MissedAnomalies);
        }

        void HideGameInterface()
        {
            hunt.Stop();
            dialogue.Hide();
            jumpscare.Hide();
            fader.ClearCard();
        }
    }
}
