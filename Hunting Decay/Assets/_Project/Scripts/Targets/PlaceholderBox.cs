using TMPro;
using UnityEngine;

namespace HuntingDecay
{
    /// <summary>
    /// Stand-in for cutout art: a tinted box with a name on it. When the art arrives, assign the sprites
    /// (on the SpriteRenderer, ZoomReveal or DaughterCutout), then delete this component and its label.
    /// </summary>
    public class PlaceholderBox : MonoBehaviour
    {
        [SerializeField] SpriteRenderer box;
        [SerializeField] TMP_Text label;
        [SerializeField] string text;
        [SerializeField] Color color = new(1f, 1f, 1f, 0.35f);

        [Header("Alternate state (revealed or infected)")]
        [Tooltip("Invisible until the alternate state, like the wendigo before it's scoped.")]
        [SerializeField] bool hiddenUntilAlternate;
        [SerializeField] string alternateText;
        [SerializeField] Color alternateColor = new(0.85f, 0.1f, 0.1f, 0.5f);

        public void SetAlternate(bool alternate)
        {
            label.text = alternate ? alternateText : text;
            box.color = alternate ? alternateColor : color;
            bool visible = alternate || !hiddenUntilAlternate;
            box.enabled = visible;
            label.enabled = visible;
        }
    }
}
