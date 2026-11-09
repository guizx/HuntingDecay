using System;
using UnityEngine;

namespace HuntingDecay
{
    /// <summary>One photograph: a root prefab made of SpriteRenderer layers.</summary>
    public class View : MonoBehaviour
    {
        [Tooltip("The photo the camera frames.")]
        [SerializeField] protected SpriteRenderer background;
        [Tooltip("Loops while this photo is open during the hunt, on top of the day's forest ambience, like the river.")]
        [SerializeField] Sound ambience;

        public SpriteRenderer Background => background;

        public Sound Ambience => ambience;

        /// <summary>For views with more than one photo, like the house by day and by night.</summary>
        protected void Select(ViewVariant selected, params ViewVariant[] all)
        {
            foreach (ViewVariant variant in all)
                variant.root.SetActive(variant == selected);
            background = selected.background;
        }
    }

    [Serializable]
    public class ViewVariant
    {
        public GameObject root;
        public SpriteRenderer background;
    }
}
