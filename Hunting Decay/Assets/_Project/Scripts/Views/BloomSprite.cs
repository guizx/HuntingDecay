using UnityEngine;

namespace HuntingDecay
{
    /// <summary>
    /// Swaps the sprite as the Bloom spreads, like the Daughter growing flowers after each missed anomaly.
    /// Stage N shows at N missed anomalies, and the last stage also covers any higher count.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class BloomSprite : MonoBehaviour, IDayStateListener
    {
        [SerializeField] SpriteRenderer spriteRenderer;
        [Tooltip("Element 0 is the healthy look. Each next element is one more missed anomaly.")]
        [SerializeField] Sprite[] stages;

        public void OnDayState(int day, int missedAnomalies) =>
            spriteRenderer.sprite = stages[Mathf.Min(missedAnomalies, stages.Length - 1)];
    }
}
