using UnityEngine;

namespace HuntingDecay
{
    /// <summary>
    /// Music that takes over once its photo is seen during the hunt, like the plant monster's theme at the
    /// Day 5 river. It keeps looping in every photo until the shot or quitting to the title.
    /// Pair it with DayVisibility so it only exists on its day.
    /// </summary>
    public class HuntMusic : MonoBehaviour
    {
        [SerializeField] Sound sound;

        public Sound Sound => sound;
    }
}
