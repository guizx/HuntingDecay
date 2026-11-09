using System;
using UnityEngine;

namespace HuntingDecay
{
    /// <summary>The card on black that names the ending, over a darkened picture.</summary>
    [Serializable]
    public class EndingCard
    {
        public bool good;
        public string name;
        [Tooltip("Its place in the list, shown as \"(number of total)\".")]
        [Min(1)] public int number = 1;

        [Header("Background")]
        public Sprite background;
        [Tooltip("1 fits the whole image to the screen. Higher zooms in on Focus. Assumes a 16:9 image.")]
        [Min(1f)] public float zoom = 1f;
        [Tooltip("The point of the image kept at the center of the screen, from (0, 0) bottom left to (1, 1) top right.")]
        public Vector2 focus = new(0.5f, 0.5f);
        [Tooltip("How bright the picture shows behind the text. 0 is black.")]
        [Range(0f, 1f)] public float brightness = 0.3f;
    }
}
