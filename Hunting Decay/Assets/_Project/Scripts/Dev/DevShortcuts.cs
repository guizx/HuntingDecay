using UnityEngine;
using UnityEngine.InputSystem;

namespace HuntingDecay
{
    /// <summary>
    /// Testing keys, active only in the Editor and development builds.
    /// F1 to F5 start that day. F6, F7 and F8 set the missed anomalies to 0, 1 and 2.
    /// In a WebGL dev build the browser keeps F5 for itself (reload).
    /// </summary>
    public class DevShortcuts : MonoBehaviour
    {
        static readonly Key[] DayKeys = { Key.F1, Key.F2, Key.F3, Key.F4, Key.F5 };
        static readonly Key[] MissedKeys = { Key.F6, Key.F7, Key.F8 };

        [SerializeField] GameFlow flow;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
                return;
            for (int i = 0; i < DayKeys.Length; i++)
            {
                if (keyboard[DayKeys[i]].wasPressedThisFrame)
                    flow.StartRun(i + 1, flow.State.MissedAnomalies);
            }
            for (int i = 0; i < MissedKeys.Length; i++)
            {
                if (keyboard[MissedKeys[i]].wasPressedThisFrame)
                    flow.SetMissedAnomalies(i);
            }
        }
#endif
    }
}
