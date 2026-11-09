using System;
using UnityEngine;
using UnityEngine.UI;

namespace HuntingDecay
{
    public class TitleScreen : MonoBehaviour
    {
        [SerializeField] GameObject root;
        [Tooltip("The title and its buttons, hidden while the credits are open.")]
        [SerializeField] GameObject menu;
        [SerializeField] GameObject credits;
        [SerializeField] Button newGameButton;
        [SerializeField] Button creditsButton;
        [SerializeField] Button exitButton;
        [SerializeField] Button creditsBackButton;

        public event Action NewGameClicked;

        void Awake()
        {
            newGameButton.onClick.AddListener(() => NewGameClicked?.Invoke());
            creditsButton.onClick.AddListener(() => ShowCredits(true));
            creditsBackButton.onClick.AddListener(() => ShowCredits(false));
            exitButton.onClick.AddListener(Quit);
#if UNITY_WEBGL && !UNITY_EDITOR
            // A browser tab has nothing to quit to.
            exitButton.gameObject.SetActive(false);
#endif
        }

        public void Show()
        {
            ShowCredits(false);
            root.SetActive(true);
        }

        public void Hide() => root.SetActive(false);

        void ShowCredits(bool show)
        {
            menu.SetActive(!show);
            credits.SetActive(show);
        }

        static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
