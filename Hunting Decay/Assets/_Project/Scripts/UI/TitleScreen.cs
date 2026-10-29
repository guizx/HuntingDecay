using System;
using UnityEngine;
using UnityEngine.UI;

namespace HuntingDecay
{
    public class TitleScreen : MonoBehaviour
    {
        [SerializeField] GameObject root;
        [SerializeField] Button newGameButton;
        [SerializeField] Button exitButton;

        public event Action NewGameClicked;

        void Awake()
        {
            newGameButton.onClick.AddListener(() => NewGameClicked?.Invoke());
            exitButton.onClick.AddListener(Quit);
#if UNITY_WEBGL && !UNITY_EDITOR
            // A browser tab has nothing to quit to.
            exitButton.gameObject.SetActive(false);
#endif
        }

        public void Show() => root.SetActive(true);

        public void Hide() => root.SetActive(false);

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
