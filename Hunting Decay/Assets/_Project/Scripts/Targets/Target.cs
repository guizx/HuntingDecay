using System;
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

        SpriteRenderer spriteRenderer;

        public TargetType Type => type;
        public string ScopePhrase => scopePhrase;
        public Jumpscare Jumpscare => jumpscare;

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
        public Sound sound;
    }
}
