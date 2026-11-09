using UnityEngine;

namespace HuntingDecay
{
    public static class UiFraming
    {
        /// <summary>
        /// Scales a full-screen image around a focus point. The anchors give the image's size in screens, so
        /// zoom 1 with the focus in the middle is the full screen. Offsets, like a shake margin, are kept.
        /// Assumes the image has the screen's aspect ratio.
        /// </summary>
        public static void Frame(RectTransform rect, float zoom, Vector2 focus)
        {
            Vector2 min = new Vector2(0.5f, 0.5f) - focus * zoom;
            rect.anchorMin = min;
            rect.anchorMax = min + Vector2.one * zoom;
        }
    }
}
