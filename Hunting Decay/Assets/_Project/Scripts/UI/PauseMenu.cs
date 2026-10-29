using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace HuntingDecay
{
    /// <summary>Esc or P. P is there because browsers use Esc to leave fullscreen.</summary>
    public class PauseMenu : MonoBehaviour
    {
        public static bool IsPaused { get; private set; }

        [SerializeField] GameObject root;
        [SerializeField] Button resumeButton;
        [SerializeField] Slider volumeSlider;
        [SerializeField] Button quitToTitleButton;
        [SerializeField] InputAction pauseAction = CreatePauseAction();

        /// <summary>Off on the title screen.</summary>
        public bool CanPause { get; set; }

        public event Action QuitToTitleClicked;

        void Awake()
        {
            resumeButton.onClick.AddListener(() => SetPaused(false));
            quitToTitleButton.onClick.AddListener(() =>
            {
                SetPaused(false);
                QuitToTitleClicked?.Invoke();
            });
            root.SetActive(false);
        }

        void Start()
        {
            volumeSlider.SetValueWithoutNotify(AudioManager.Instance.MasterVolume);
            volumeSlider.onValueChanged.AddListener(AudioManager.Instance.SetMasterVolume);
        }

        void OnEnable() => pauseAction.Enable();

        void OnDisable() => pauseAction.Disable();

        void OnDestroy() => SetPaused(false);

        void Update()
        {
            if (pauseAction.WasPressedThisFrame() && (CanPause || IsPaused))
                SetPaused(!IsPaused);
        }

        void SetPaused(bool paused)
        {
            IsPaused = paused;
            root.SetActive(paused);
            Time.timeScale = paused ? 0f : 1f;
            AudioListener.pause = paused;
        }

        static InputAction CreatePauseAction()
        {
            var action = new InputAction("Pause", InputActionType.Button, "<Keyboard>/escape");
            action.AddBinding("<Keyboard>/p");
            return action;
        }
    }
}
