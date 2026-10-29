using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HuntingDecay
{
    public class JumpscareScreen : MonoBehaviour
    {
        [SerializeField] GameObject root;
        [SerializeField] Image image;
        [SerializeField] TMP_Text placeholderLabel;
        [SerializeField] Color placeholderColor = new(0.55f, 0.05f, 0.05f, 1f);

        public IEnumerator Play(Jumpscare jumpscare, float hold)
        {
            bool hasImage = jumpscare.image != null;
            image.sprite = jumpscare.image;
            image.color = hasImage ? Color.white : placeholderColor;
            placeholderLabel.text = hasImage ? string.Empty : jumpscare.placeholderLabel;
            root.SetActive(true);
            AudioManager.Instance.Play(jumpscare.sound);
            yield return new WaitForSeconds(hold);
        }

        public void Hide() => root.SetActive(false);
    }
}
