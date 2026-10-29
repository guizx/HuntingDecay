using TMPro;
using UnityEngine;

namespace HuntingDecay
{
    public class Subtitles : MonoBehaviour
    {
        [SerializeField] TMP_Text text;
        [SerializeField, Min(0.5f)] float duration = 2.5f;

        float hideAt;

        public void Show(string caption)
        {
            text.text = caption;
            text.enabled = true;
            hideAt = Time.time + duration;
        }

        public void Hide(string caption)
        {
            if (text.enabled && text.text == caption)
                text.enabled = false;
        }

        void Update()
        {
            if (text.enabled && Time.time >= hideAt)
                text.enabled = false;
        }
    }
}
