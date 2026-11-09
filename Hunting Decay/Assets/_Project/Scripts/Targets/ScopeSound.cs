using UnityEngine;

namespace HuntingDecay
{
    /// <summary>
    /// Plays a sound the first time each day the scope lands on this target, like an anomaly's scare,
    /// Dr Marsh's gasp or the wolf's growl.
    /// </summary>
    public class ScopeSound : MonoBehaviour, IDayStateListener
    {
        [SerializeField] Sound sound;

        bool playedToday;

        public void Play()
        {
            if (playedToday)
                return;
            playedToday = true;
            AudioManager.Instance.StopRandom();
            AudioManager.Instance.Play(sound);
        }

        public void OnDayState(int day, int missedAnomalies) => playedToday = false;
    }
}
