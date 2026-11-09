using System.Collections.Generic;
using UnityEngine;

namespace HuntingDecay
{
    /// <summary>
    /// Unsettling sounds in the forest, like a distant whale or someone humming. A few seconds into the hunt
    /// one plays, then a second one much later, and never a third on the same day. Anomaly sounds cut
    /// them short, since ScopeSound and AmbientCue call AudioManager.StopRandom.
    /// </summary>
    public class RandomSounds : MonoBehaviour
    {
        [Tooltip("One is picked at random each time, never the same one twice in a day.")]
        [SerializeField] List<Sound> sounds = new();
        [Tooltip("Seconds into the hunt before the first sound, picked at random in this range.")]
        [SerializeField] Vector2 firstDelay = new(10f, 20f);
        [Tooltip("Seconds of silence after a sound ends before the next one, picked at random in this range.")]
        [SerializeField] Vector2 gap = new(40f, 60f);
        [SerializeField, Min(0)] int maxPerDay = 2;

        readonly List<Sound> unplayed = new();
        float timer;
        int playedToday;
        bool running;
        // The gap starts when the sound actually stops, which is early when an anomaly cuts it short.
        bool waitingForEnd;

        /// <summary>Starts a new day's schedule. The count resets here, so each day gets its own sounds.</summary>
        public void Begin(bool enabledToday)
        {
            running = enabledToday && sounds.Count > 0;
            playedToday = 0;
            waitingForEnd = false;
            unplayed.Clear();
            unplayed.AddRange(sounds);
            timer = Random.Range(firstDelay.x, firstDelay.y);
        }

        public void Stop()
        {
            running = false;
            AudioManager.Instance.StopRandom();
        }

        void Update()
        {
            if (!running || playedToday >= maxPerDay)
                return;
            if (waitingForEnd)
            {
                if (AudioManager.Instance.IsRandomPlaying)
                    return;
                waitingForEnd = false;
                timer = Random.Range(gap.x, gap.y);
            }
            // Scaled time, so the pause menu freezes the countdown too.
            timer -= Time.deltaTime;
            if (timer > 0f)
                return;

            Sound sound = unplayed[Random.Range(0, unplayed.Count)];
            unplayed.Remove(sound);
            playedToday++;
            AudioManager.Instance.PlayRandom(sound);
            waitingForEnd = true;
        }
    }
}
