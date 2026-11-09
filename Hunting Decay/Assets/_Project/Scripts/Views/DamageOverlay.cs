using UnityEngine;

namespace HuntingDecay
{
    /// <summary>The aged-photo overlay. One per view, sorted above everything in the photo.</summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class DamageOverlay : MonoBehaviour, IDayStateListener
    {
        /// <summary>How strong the damage is in this view, set by the sprite's alpha.</summary>
        public float Opacity => GetComponent<SpriteRenderer>().color.a;

        public void OnDayState(int day, int missedAnomalies) => gameObject.SetActive(missedAnomalies > 0);
    }
}
