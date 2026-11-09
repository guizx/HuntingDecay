using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace HuntingDecay
{
    /// <summary>
    /// The black screen between scenes, the shot flash, and the text cards shown on black. The black can also
    /// carry the aged-photo damage, like when the Bloom spreads.
    /// </summary>
    public class ScreenFader : MonoBehaviour
    {
        [SerializeField] CanvasGroup black;
        [SerializeField] CanvasGroup flash;
        [SerializeField] Typewriter card;
        [Tooltip("The damage overlay drawn on the black, under the card. It's a child of the black, so it fades "
            + "out with it, and it's cleared once the black is gone.")]
        [SerializeField] CanvasGroup damage;
        [Tooltip("A darkened picture behind a card, like on the ending cards. A child of the black, under the damage.")]
        [SerializeField] Image cardBackground;
        [SerializeField] CanvasGroup cardBackgroundGroup;

        public void SetBlack(bool on)
        {
            black.alpha = on ? 1f : 0f;
            if (!on)
                damage.alpha = 0f;
        }

        public IEnumerator FadeToBlack(float duration) => Fade(black, 1f, duration);

        public IEnumerator FadeFromBlack(float duration)
        {
            yield return Fade(black, 0f, duration);
            damage.alpha = 0f;
        }

        public void SetDamage(float opacity) => damage.alpha = opacity;

        public IEnumerator FadeInDamage(float opacity, float duration) => Fade(damage, opacity, duration);

        /// <summary>The shot: white that fades straight into black, so the scene never shows again.</summary>
        public IEnumerator FlashToBlack(float duration)
        {
            // The flash sits above the black, so fading it out reveals the black underneath.
            black.alpha = 1f;
            flash.alpha = 1f;
            yield return Fade(flash, 0f, duration);
        }

        /// <summary>Types the card text and returns once it's fully shown. Call ClearCard to remove it.</summary>
        public IEnumerator TypeCard(string text)
        {
            card.Play(text);
            while (card.IsTyping)
                yield return null;
        }

        /// <summary>
        /// Types only the first count characters of the card, then waits one more letter's time, so whatever
        /// comes next lands where the next letter would have.
        /// </summary>
        public IEnumerator TypeCardUntil(string text, int count)
        {
            card.Play(text, stopAfter: count);
            while (card.IsTyping)
                yield return null;
            yield return new WaitForSeconds(card.LetterInterval);
        }

        /// <summary>Fades in a picture behind the next card. ClearCard removes it with the card.</summary>
        public IEnumerator FadeInCardBackground(Sprite picture, float zoom, Vector2 focus, float brightness, float duration)
        {
            cardBackground.sprite = picture;
            cardBackground.color = new Color(brightness, brightness, brightness, 1f);
            UiFraming.Frame(cardBackground.rectTransform, zoom, focus);
            cardBackgroundGroup.alpha = 0f;
            yield return Fade(cardBackgroundGroup, 1f, duration);
        }

        public void ClearCard()
        {
            card.Clear();
            cardBackgroundGroup.alpha = 0f;
        }

        static IEnumerator Fade(CanvasGroup group, float to, float duration)
        {
            float from = group.alpha;
            for (float time = 0f; time < duration && !Mathf.Approximately(group.alpha, to); time += Time.deltaTime)
            {
                group.alpha = Mathf.Lerp(from, to, time / duration);
                yield return null;
            }
            group.alpha = to;
        }
    }
}
