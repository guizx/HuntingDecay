using System.Collections.Generic;
using UnityEngine;

namespace HuntingDecay
{
    /// <summary>Every target is already placed in its view. This turns it on only on the days it's used.</summary>
    public class DayVisibility : MonoBehaviour, IDayStateListener
    {
        [SerializeField] List<int> days = new() { 1, 2, 3, 4, 5 };

        public void OnDayState(int day, int missedAnomalies) => gameObject.SetActive(days.Contains(day));
    }
}
