using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace HuntingDecay
{
    /// <summary>The texts around the scope, plus the tutorial hint, which also shows outside the scope.</summary>
    public class ScopeHud : MonoBehaviour
    {
        [FormerlySerializedAs("bulletText")]
        [SerializeField] TMP_Text noteText;
        [SerializeField] Typewriter phrase;
        [SerializeField] Typewriter shootPrompt;
        [SerializeField] Typewriter hint;

        /// <summary>The day's note in the scope's corner. Hidden when the day has none.</summary>
        public void ShowNote(string text)
        {
            noteText.text = text;
            noteText.gameObject.SetActive(!string.IsNullOrEmpty(text));
        }

        public void ShowTarget(Target target, bool canShoot, string prompt)
        {
            if (target == null)
            {
                phrase.Clear();
                shootPrompt.Clear();
                return;
            }
            phrase.Play(target.ScopePhrase);
            shootPrompt.Play(canShoot ? prompt : string.Empty);
        }

        public void ShowHint(string text) => hint.Play(text);

        public void HideHint() => hint.Clear();
    }
}
