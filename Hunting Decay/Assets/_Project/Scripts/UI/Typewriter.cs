using TMPro;
using UnityEngine;

namespace HuntingDecay
{
    /// <summary>Reveals a TextMeshPro text one letter at a time, with an optional sound per letter.</summary>
    public class Typewriter : MonoBehaviour
    {
        [SerializeField] TMP_Text text;
        [SerializeField, Min(1f)] float charactersPerSecond = 40f;

        [Tooltip("Plays the typing sound. Its clips, volume, pitch and rhythm are set once on the AudioManager.")]
        [SerializeField] bool typeSound;

        float shown;
        int total;
        int lettersUntilSound;
        bool needsCount;
        Sound voice;

        public bool IsTyping => needsCount || shown < total;

        /// <param name="voice">Typing sound for this text only, like a speaker's voice. Null uses the default.</param>
        public void Play(string value, Sound voice = null)
        {
            this.voice = voice;
            text.text = value;
            text.maxVisibleCharacters = 0;
            shown = 0f;
            // The first letter makes a sound.
            lettersUntilSound = 1;
            // The character count is only known after TextMeshPro parses the text, which needs the object active.
            needsCount = true;
        }

        public void Complete()
        {
            CountCharacters();
            shown = total;
            text.maxVisibleCharacters = total;
        }

        public void Clear() => Play(string.Empty);

        void Update()
        {
            if (needsCount)
                CountCharacters();
            if (shown >= total)
                return;

            int before = (int)shown;
            shown = Mathf.Min(total, shown + charactersPerSecond * Time.deltaTime);
            int after = (int)shown;
            for (int i = before; i < after; i++)
            {
                if (!typeSound || char.IsWhiteSpace(text.textInfo.characterInfo[i].character))
                    continue;
                if (--lettersUntilSound > 0)
                    continue;
                lettersUntilSound = AudioManager.Instance.LettersUntilNextTypeSound();
                AudioManager.Instance.PlayTypeSound(voice);
            }
            text.maxVisibleCharacters = after;
        }

        void CountCharacters()
        {
            if (!needsCount)
                return;
            text.ForceMeshUpdate();
            total = text.textInfo.characterCount;
            needsCount = false;
        }
    }
}
