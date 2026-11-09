using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HuntingDecay
{
    /// <summary>
    /// The face that jumps on screen. It sits on an opaque black backdrop and is drawn a little larger than
    /// the screen, so the shake never shows the game underneath.
    /// </summary>
    public class JumpscareScreen : MonoBehaviour
    {
        [SerializeField] GameObject root;
        [Tooltip("The face. The root's own image is the black backdrop behind it.")]
        [SerializeField] Image image;
        [SerializeField] TMP_Text placeholderLabel;
        [SerializeField] Color placeholderColor = new(0.55f, 0.05f, 0.05f, 1f);

        [Header("Shake")]
        [Tooltip("How far the face jolts from center, in canvas pixels (1920x1080 reference). 0 turns it off.")]
        [SerializeField, Min(0f)] float shakePixels = 4f;
        [Tooltip("Seconds between jolts. Lower is more frantic.")]
        [SerializeField, Min(0.01f)] float shakeInterval = 0.04f;

        public IEnumerator Play(Jumpscare jumpscare, float hold)
        {
            bool hasImage = jumpscare.image != null;
            image.sprite = jumpscare.image;
            image.color = hasImage ? Color.white : placeholderColor;
            placeholderLabel.text = hasImage ? string.Empty : jumpscare.placeholderLabel;
            root.SetActive(true);
            AudioManager.Instance.Play(jumpscare.sound);

            RectTransform face = image.rectTransform;
            float end = Time.time + hold;
            float nextJolt = 0f;
            while (Time.time < end)
            {
                if (Time.time >= nextJolt)
                {
                    face.anchoredPosition = Random.insideUnitCircle * shakePixels;
                    nextJolt = Time.time + shakeInterval;
                }
                yield return null;
            }
            face.anchoredPosition = Vector2.zero;
        }

        public void Hide() => root.SetActive(false);
    }
}
