using UnityEngine;
using UnityEngine.Events;

namespace HuntingDecay
{
    /// <summary>The Daughter at the table. She shows plants on her skin from the first missed anomaly.</summary>
    public class DaughterCutout : MonoBehaviour, IDayStateListener
    {
        [SerializeField] SpriteRenderer spriteRenderer;
        [SerializeField] Sprite healthySprite;
        [SerializeField] Sprite infectedSprite;
        [SerializeField] UnityEvent<bool> infectedChanged = new();

        public void OnDayState(int day, int missedAnomalies)
        {
            bool infected = missedAnomalies > 0;
            spriteRenderer.sprite = infected ? infectedSprite : healthySprite;
            infectedChanged.Invoke(infected);
        }
    }
}
