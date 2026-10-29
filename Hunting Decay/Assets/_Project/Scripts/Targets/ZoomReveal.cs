using UnityEngine;
using UnityEngine.Events;

namespace HuntingDecay
{
    /// <summary>
    /// Swaps the sprite while the scope is on this object, so the anomaly reacts to being seen, and swaps
    /// back when the player stops zooming. It's only ever a sprite swap, never an animation.
    /// The sound that goes with it is a separate ScopeSound.
    /// </summary>
    public class ZoomReveal : MonoBehaviour, IDayStateListener
    {
        [SerializeField] SpriteRenderer spriteRenderer;
        [SerializeField] Sprite hiddenSprite;
        [SerializeField] Sprite revealedSprite;
        [SerializeField] UnityEvent<bool> revealedChanged = new();

        public bool IsRevealed { get; private set; }

        public void Reveal()
        {
            if (!IsRevealed)
                SetRevealed(true);
        }

        public void Hide()
        {
            if (IsRevealed)
                SetRevealed(false);
        }

        public void OnDayState(int day, int missedAnomalies) => SetRevealed(false);

        void SetRevealed(bool revealed)
        {
            IsRevealed = revealed;
            spriteRenderer.sprite = revealed ? revealedSprite : hiddenSprite;
            revealedChanged.Invoke(revealed);
        }
    }
}
