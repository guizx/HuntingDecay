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
        /// <summary>Marks where the jumpscare cuts into a Bloom card. It's never shown.</summary>
        const char CardCut = '|';

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
        [Tooltip("The camera rig, which also shakes the view.")]
        [SerializeField] Scope scope;

        [Header("Timing (seconds)")]
        [SerializeField, Min(0f)] float fadeDuration = 0.8f;
        [SerializeField, Min(0f)] float dayCardHold = 1.2f;
        [Tooltip("How long the shot's white flash takes to fade into black.")]
        [SerializeField, Min(0f)] float flashToBlackDuration = 1f;
        [SerializeField, Min(0f)] float jumpscareHold = 0.9f;
        [Tooltip("How long lingering effects, like a scope sound or the jumpscare, take to fade out when the house appears.")]
        [SerializeField, Min(0f)] float lingeringSoundFade = 0.8f;
        [Tooltip("How long the damage takes to cover the black while the first Bloom card types.")]
        [SerializeField, Min(0f)] float bloomDamageFade = 2.5f;

        [Header("Dinner shake")]
        [Tooltip("Seconds the view shakes as the changed daughter is first seen, after each Bloom spread. It starts "
            + "with the dinner's fade in, so it should outlast Fade Duration.")]
        [SerializeField, Min(0f)] float dinnerShakeDuration = 1.2f;
        [Tooltip("How far the view jolts, in screen pixels, easing to rest.")]
        [SerializeField, Min(0f)] float dinnerShakePixels = 6f;

        public GameState State => state;

        /// <summary>The house is silent until the Bloom spreads.</summary>
        Sound HouseMusic => state.MissedAnomalies > 0 ? story.infectedHouseMusic : null;

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
            atmosphere.ApplyTitle(story.titleLook, story.titleMusic);
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
            atmosphere.StopWeather();
            HideGameInterface();

            while (true)
            {
                DayData day = story.days[state.Day - 1];
                ApplyState();
                atmosphere.ApplyDay(day);
                // Between the house scenes, so dinner's music carries through to breakfast.
                AudioManager.Instance.PlayMusic(HouseMusic);
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
                    yield return PlayBloomCard(string.IsNullOrEmpty(hit.BloomCard) ? story.missedAnomalyCard : hit.BloomCard);
                }

                if (result.Ending != Ending.None)
                {
                    yield return PlayEnding(result.Ending);
                    yield break;
                }

                if (state.MissedAnomalies >= GameState.MissedAnomaliesForEnding)
                {
                    // The ending's music starts here and carries on into the ending.
                    yield return PlayDinner(story.lastDinner, NoLines, story.badEndingMusic, shake: true);
                    yield return PlayEnding(Ending.EndOfHollowPine);
                    yield break;
                }

                Dinner dinner = state.MissedAnomalies == 0 ? day.dinnerHealthy : day.dinnerInfected;
                bool firstSpread = result.BloomAdvances && state.MissedAnomalies == 1;
                yield return PlayDinner(dinner, result.BloomAdvances ? day.missedNews : NoLines, HouseMusic,
                    firstSpread ? story.firstInfectedDinnerSound : null, shake: firstSpread);
                state.NextDay();
            }
        }

        IEnumerator PlayBreakfast(DayData day)
        {
            ShowHouse(HouseScene.Breakfast);
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
                // No shot at all: the face is on screen the moment the player clicks, then it cuts to dinner.
                hunt.Stop();
                yield return jumpscare.Play(hit.Jumpscare, jumpscareHold);
                fader.SetBlack(true);
                jumpscare.Hide();
            }
            else
            {
                yield return fader.FlashToBlack(flashToBlackDuration);
                hunt.Stop();
            }
            atmosphere.LeaveForest();
        }

        /// <summary>
        /// The card on black after the Bloom spreads, with the damage covering the black. The first time the
        /// damage fades in while the card types. The last time it's already there, and the jumpscare cuts the
        /// card off at its mark, then it goes straight on to the dinner.
        /// </summary>
        IEnumerator PlayBloomCard(string card)
        {
            int cut = card.IndexOf(CardCut);
            string text = card.Replace(CardCut.ToString(), string.Empty);
            // The same strength as the dinner it fades into.
            float damage = houseView.DamageOpacity;

            if (state.MissedAnomalies < GameState.MissedAnomaliesForEnding)
            {
                Coroutine fade = StartCoroutine(fader.FadeInDamage(damage, bloomDamageFade));
                yield return fader.TypeCard(text);
                yield return new WaitForSeconds(dayCardHold);
                yield return fade;
                fader.ClearCard();
                yield break;
            }

            fader.SetDamage(damage);
            yield return fader.TypeCardUntil(text, cut >= 0 ? cut : text.Length);
            fader.ClearCard();
            yield return jumpscare.Play(story.bloomJumpscare, jumpscareHold);
            jumpscare.Hide();
        }

        /// <param name="music">Starts as the dinner appears.</param>
        /// <param name="arrival">An effect that plays as the dinner appears.</param>
        /// <param name="shake">Shakes the view from the dinner's first frame, as the changed daughter is seen.</param>
        IEnumerator PlayDinner(Dinner dinner, List<DialogueLine> news, Sound music, Sound arrival = null, bool shake = false)
        {
            ShowHouse(HouseScene.Dinner);
            // After ShowHouse, which fades out the effects that were already playing.
            AudioManager.Instance.Play(arrival);
            AudioManager.Instance.PlayMusic(music);
            // Already shaking on the first frame the dinner shows through the black, in sync with the arrival.
            if (shake)
                scope.Shake(dinnerShakeDuration, dinnerShakePixels);
            atmosphere.SetNight(true);
            yield return fader.FadeFromBlack(fadeDuration);

            yield return dialogue.Play(dinner.question);
            int choice = 0;
            yield return dialogue.Choose(dinner.replies.Select(reply => reply.text).ToList(), index => choice = index);
            yield return dialogue.Play(dinner.replies[choice].reaction.Concat(news));
            dialogue.Hide();
            yield return fader.FadeToBlack(fadeDuration);
            atmosphere.SetNight(false);
        }

        IEnumerator PlayEnding(Ending ending)
        {
            List<DialogueLine> lines;
            EndingCard card;
            switch (ending)
            {
                case Ending.HappyWinter:
                    endingView.ShowHappyWinter();
                    viewManager.Show(endingView);
                    atmosphere.ApplyHuntGrade();
                    bool noLoss = state.MissedAnomalies == 0;
                    lines = noLoss ? story.happyWinter : story.happyWinterWithLoss;
                    card = noLoss ? story.happyWinterCard : story.bittersweetWinterCard;
                    AudioManager.Instance.PlayMusic(story.plantEndingMusic);
                    break;
                case Ending.Withering:
                    endingView.ShowWithering();
                    viewManager.Show(endingView);
                    atmosphere.ApplyHuntGrade();
                    lines = story.withering;
                    card = story.witheringCard;
                    AudioManager.Instance.PlayMusic(story.badEndingMusic);
                    break;
                default:
                    ShowHouse(HouseScene.Overgrown);
                    lines = story.endOfHollowPine;
                    card = story.endOfHollowPineCard;
                    AudioManager.Instance.PlayMusic(story.badEndingMusic);
                    break;
            }

            yield return fader.FadeFromBlack(fadeDuration);
            yield return dialogue.Play(lines);
            dialogue.Hide();
            yield return fader.FadeToBlack(fadeDuration);
            yield return fader.FadeInCardBackground(card.background, card.zoom, card.focus, card.brightness, fadeDuration);
            yield return fader.TypeCard(story.EndingCardText(card));
            yield return dialogue.WaitForClick();
            fader.ClearCard();
            ShowTitle();
        }

        void ShowHouse(HouseScene scene)
        {
            AudioManager.Instance.FadeOutEffects(lingeringSoundFade);
            houseView.SetScene(scene);
            viewManager.Show(houseView);
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
