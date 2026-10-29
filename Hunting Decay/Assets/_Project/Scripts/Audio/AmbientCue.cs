using UnityEngine;

namespace HuntingDecay
{
    /// <summary>
    /// A sound that plays while its photo is open during the hunt, like the voice calling for help in the
    /// Day 4 cave. The hunt runs it, so it stops, caption included, when the player changes photos or shoots.
    /// </summary>
    public class AmbientCue : MonoBehaviour, IDayStateListener
    {
        [SerializeField] Sound sound;
        [Tooltip("Subtitle shown each time it plays.")]
        [SerializeField] string caption;
        [Tooltip("On: plays every Interval seconds. Off: plays only the first time the photo opens each day.")]
        [SerializeField] bool repeat = true;
        [SerializeField, Min(0.5f)] float interval = 7f;
        [Tooltip("Delay before it plays after the photo opens.")]
        [SerializeField, Min(0f)] float firstDelay = 1.5f;

        float timer;
        bool playedToday;

        public void Begin() => timer = firstDelay;

        public void Tick(float deltaTime)
        {
            if (!repeat && playedToday)
                return;
            timer -= deltaTime;
            if (timer > 0f)
                return;
            timer = interval;
            playedToday = true;
            AudioManager.Instance.Play(sound, caption);
        }

        public void Stop() => AudioManager.Instance.HideCaption(caption);

        public void OnDayState(int day, int missedAnomalies) => playedToday = false;
    }
}
