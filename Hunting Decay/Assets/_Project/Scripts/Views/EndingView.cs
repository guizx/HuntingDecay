using UnityEngine;

namespace HuntingDecay
{
    /// <summary>The final-day ending photos. The End of Hollow Pine uses the overgrown house instead.</summary>
    public class EndingView : View
    {
        [SerializeField] ViewVariant happyWinter;
        [SerializeField] ViewVariant withering;

        public void ShowHappyWinter() => Select(happyWinter, happyWinter, withering);

        public void ShowWithering() => Select(withering, happyWinter, withering);
    }
}
