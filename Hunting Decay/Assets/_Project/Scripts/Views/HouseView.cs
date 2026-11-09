using UnityEngine;

namespace HuntingDecay
{
    public enum HouseScene { Breakfast, Dinner, Overgrown }

    public class HouseView : View
    {
        [SerializeField] ViewVariant breakfast;
        [SerializeField] ViewVariant dinner;
        [Tooltip("The empty house covered by the Bloom, for The End of Hollow Pine.")]
        [SerializeField] ViewVariant overgrown;

        /// <summary>The house's damage strength, so the Bloom card can end on the same look as the dinner.</summary>
        public float DamageOpacity => GetComponentInChildren<DamageOverlay>(true).Opacity;

        public void SetScene(HouseScene scene)
        {
            ViewVariant selected = scene switch
            {
                HouseScene.Breakfast => breakfast,
                HouseScene.Dinner => dinner,
                _ => overgrown,
            };
            Select(selected, breakfast, dinner, overgrown);
        }
    }
}
