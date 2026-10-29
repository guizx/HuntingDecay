using System;
using UnityEngine;

namespace HuntingDecay
{
    [Serializable]
    public class GameState
    {
        public const int MissedAnomaliesForEnding = 2;

        [SerializeField, Min(1)] int day = 1;
        [Tooltip("The Bloom.")]
        [SerializeField, Min(0)] int missedAnomalies;

        public int Day => day;
        public int MissedAnomalies => missedAnomalies;

        public void Set(int newDay, int newMissedAnomalies)
        {
            day = newDay;
            missedAnomalies = newMissedAnomalies;
        }

        public void NextDay() => day++;

        public void AddMissedAnomaly() => missedAnomalies++;
    }
}
