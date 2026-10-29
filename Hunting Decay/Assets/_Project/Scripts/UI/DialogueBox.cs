using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace HuntingDecay
{
    /// <summary>
    /// The text box at the bottom of the house scenes and endings. A click finishes the line being
    /// typed, and the next click moves on.
    /// </summary>
    public class DialogueBox : MonoBehaviour
    {
        [SerializeField] GameObject panel;
        [SerializeField] GameObject nameTag;
        [SerializeField] TMP_Text nameText;
        [SerializeField] Typewriter body;
        [SerializeField] GameObject continueMarker;
        [SerializeField] List<Button> choiceButtons = new();
        [SerializeField] InputAction advanceAction = new("Advance", InputActionType.Button, "<Mouse>/leftButton");
        [Tooltip("Typing sound per speaker name. Speakers not listed, and narration, use the AudioManager's default.")]
        [SerializeField] List<SpeakerVoice> speakerVoices = new();

        int chosen = -1;

        void Awake()
        {
            for (int i = 0; i < choiceButtons.Count; i++)
            {
                int index = i;
                choiceButtons[i].onClick.AddListener(() => chosen = index);
            }
            Hide();
        }

        void OnEnable() => advanceAction.Enable();

        void OnDisable() => advanceAction.Disable();

        public IEnumerator Play(IEnumerable<DialogueLine> lines)
        {
            panel.SetActive(true);
            SetChoicesVisible(0);
            foreach (DialogueLine line in lines)
            {
                bool hasSpeaker = !string.IsNullOrEmpty(line.speaker);
                nameTag.SetActive(hasSpeaker);
                nameText.text = line.speaker;
                body.Play(line.text, VoiceFor(line.speaker));
                continueMarker.SetActive(false);

                // The click that ended the previous step must not also skip this line.
                yield return null;
                while (true)
                {
                    continueMarker.SetActive(!body.IsTyping);
                    if (AdvancePressed())
                    {
                        if (!body.IsTyping)
                            break;
                        body.Complete();
                    }
                    yield return null;
                }
            }
            continueMarker.SetActive(false);
        }

        /// <summary>Shows the options under the last line and waits for one to be clicked.</summary>
        public IEnumerator Choose(IReadOnlyList<string> options, Action<int> onChosen)
        {
            panel.SetActive(true);
            continueMarker.SetActive(false);
            SetChoicesVisible(options.Count);
            for (int i = 0; i < options.Count; i++)
                choiceButtons[i].GetComponentInChildren<TMP_Text>().text = options[i];

            chosen = -1;
            while (chosen < 0)
                yield return null;
            SetChoicesVisible(0);
            onChosen(chosen);
        }

        public IEnumerator WaitForClick()
        {
            yield return null;
            while (!AdvancePressed())
                yield return null;
        }

        public void Hide()
        {
            panel.SetActive(false);
            SetChoicesVisible(0);
            body.Clear();
        }

        // Clicks on buttons belong to the buttons, including the pause menu's.
        bool AdvancePressed() =>
            !PauseMenu.IsPaused
            && advanceAction.WasPressedThisFrame()
            && !EventSystem.current.IsPointerOverGameObject();

        void SetChoicesVisible(int count)
        {
            for (int i = 0; i < choiceButtons.Count; i++)
                choiceButtons[i].gameObject.SetActive(i < count);
        }

        Sound VoiceFor(string speaker)
        {
            foreach (SpeakerVoice voice in speakerVoices)
            {
                if (voice.speaker == speaker)
                    return voice.typeSound;
            }
            return null;
        }
    }

    [Serializable]
    public class SpeakerVoice
    {
        [Tooltip("Must match the Speaker field of the dialogue lines exactly, like \"Daughter\".")]
        public string speaker;
        public Sound typeSound;
    }
}
