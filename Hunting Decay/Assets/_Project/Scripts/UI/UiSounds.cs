using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HuntingDecay
{
    /// <summary>Hover and click sounds for a button. The clips live on the AudioManager.</summary>
    [RequireComponent(typeof(Button))]
    public class UiSounds : MonoBehaviour, IPointerEnterHandler
    {
        Button button;

        void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(() => AudioManager.Instance.PlayClick());
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (button.IsInteractable())
                AudioManager.Instance.PlayHover();
        }
    }
}
