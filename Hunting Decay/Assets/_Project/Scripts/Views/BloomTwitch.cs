using UnityEngine;

namespace HuntingDecay
{
    /// <summary>
    /// Makes a sprite twitch once the Bloom has spread: every few seconds it jerks off its spot for a frame or
    /// two, sometimes twice in a row, then snaps back. Healthy, it stays still.
    /// </summary>
    public class BloomTwitch : MonoBehaviour, IDayStateListener
    {
        [Tooltip("Twitches from this many missed anomalies on.")]
        [SerializeField, Min(1)] int fromMissedAnomalies = 1;
        [Tooltip("How far each jerk moves the sprite, in screen pixels at 1080p.")]
        [SerializeField, Min(0f)] float pixels = 5f;
        [Tooltip("Seconds of stillness between twitches, picked at random in this range.")]
        [SerializeField] Vector2 secondsBetween = new(0.8f, 2.5f);
        [Tooltip("How many quick jerks one twitch has, picked at random in this range.")]
        [SerializeField] Vector2Int jerks = new(1, 3);
        [Tooltip("Seconds each jerk holds before the next one or the snap back.")]
        [SerializeField, Min(0.01f)] float jerkSeconds = 0.05f;

        const float ReferenceHeight = 1080f;

        Vector3 rest;
        bool twitching;
        float nextChange;
        int jerksLeft;

        void Awake() => rest = transform.localPosition;

        // Can arrive while the scene variant is inactive, before Awake, so it only sets the flag.
        public void OnDayState(int day, int missedAnomalies) => twitching = missedAnomalies >= fromMissedAnomalies;

        void OnEnable() => ScheduleTwitch();

        void OnDisable() => transform.localPosition = rest;

        void Update()
        {
            if (!twitching)
            {
                transform.localPosition = rest;
                return;
            }
            if (Time.time < nextChange)
                return;

            if (jerksLeft > 0)
            {
                jerksLeft--;
                transform.localPosition = rest + (Vector3)(Random.insideUnitCircle.normalized * UnitsPerPixel() * pixels);
                nextChange = Time.time + jerkSeconds;
                return;
            }

            transform.localPosition = rest;
            ScheduleTwitch();
        }

        void ScheduleTwitch()
        {
            jerksLeft = Random.Range(jerks.x, jerks.y + 1);
            nextChange = Time.time + Random.Range(secondsBetween.x, secondsBetween.y);
        }

        /// <summary>World units per screen pixel at 1080p, so the twitch reads the same at any resolution.</summary>
        static float UnitsPerPixel()
        {
            Camera cam = Camera.main;
            return cam != null ? 2f * cam.orthographicSize / ReferenceHeight : 0f;
        }
    }
}
