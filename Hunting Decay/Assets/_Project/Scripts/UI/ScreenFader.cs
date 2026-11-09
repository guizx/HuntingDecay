using System.Collections;
using UnityEngine;

namespace HuntingDecay
{
    /// <summary>The black screen between scenes, the shot flash, and the text cards shown on black.</summary>
    public class ScreenFader : MonoBehaviour
    {
        [SerializeField] CanvasGroup black;
        [SerializeField] CanvasGroup flash;
        [SerializeField] Typewriter card;

        public void SetBlack(bool on) => black.alpha = on ? 1f : 0f;

        public IEnumerator FadeToBlack(float duration) => Fade(black, 1f, duration);

        public IEnumerator FadeFromBlack(float duration) => Fade(black, 0f, duration);

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

        public void ClearCard() => card.Clear();

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
