using System;
using System.Collections.Generic;
using UnityEngine;

namespace HuntingDecay
{
    /// <summary>Anything the scope can describe. Whether it can be shot is decided by ShotRules.</summary>
    [RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
    public class Target : MonoBehaviour
    {
        [SerializeField] TargetType type;
        [Tooltip("Shown in the scope while aiming at this. About one sentence fits.")]
        [SerializeField, TextArea(1, 3)] string scopePhrase;
        [Tooltip("Plays instead of the fade to black after the shot, like the wendigo's face.")]
        [SerializeField] Jumpscare jumpscare = new();
        [Tooltip("The card shown when shooting this spreads the Bloom, like \"You shot Dr. M|arsh.\". Empty uses the "
            + "story's missed anomaly card. A | marks where the second spread's jumpscare cuts in.")]
        [SerializeField] string bloomCard;

        SpriteRenderer spriteRenderer;

        public TargetType Type => type;
        public string ScopePhrase => scopePhrase;
        public Jumpscare Jumpscare => jumpscare;
        public string BloomCard => bloomCard;

        /// <summary>Used to pick the front-most target when colliders overlap.</summary>
        public int SortingOrder => spriteRenderer.sortingOrder;

        void Awake() => spriteRenderer = GetComponent<SpriteRenderer>();
    }

    [Serializable]
    public class Jumpscare
    {
        public bool enabled;
        public Sprite image;
        [Tooltip("Shown on a colored box while there's no image.")]
        public string placeholderLabel;
        [Tooltip("All of them play at once.")]
        public List<Sound> sounds = new();
        [Tooltip("1 fits the whole image to the screen. Higher zooms in on Focus. Assumes a 16:9 image.")]
        [Min(1f)] public float zoom = 1f;
        [Tooltip("The point of the image kept at the center of the screen, from (0, 0) bottom left to (1, 1) top right.")]
        public Vector2 focus = new(0.5f, 0.5f);
    }
}
