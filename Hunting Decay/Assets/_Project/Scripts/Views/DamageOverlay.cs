using UnityEngine;

namespace HuntingDecay
{
    /// <summary>The aged-photo overlay. One per view, sorted above everything in the photo.</summary>
    public class DamageOverlay : MonoBehaviour, IDayStateListener
    {
        public void OnDayState(int day, int missedAnomalies) => gameObject.SetActive(missedAnomalies > 0);
    }
}
