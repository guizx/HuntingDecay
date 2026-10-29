using UnityEngine;
using UnityEngine.InputSystem;

namespace HuntingDecay
{
    /// <summary>
    /// Frames the current photo, and works as the rifle scope while aiming is allowed. While aim is held,
    /// shows the HUD and zooms the camera so the background is drawn at <see cref="zoom"/> screen pixels
    /// per texture pixel. The cursor's normalized screen position picks the same normalized point of the
    /// background, which is placed at the camera center (the aim point), plus a slow sway.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class Scope : MonoBehaviour
    {
        [SerializeField] SpriteRenderer background;
        [SerializeField] GameObject hud;

        [Tooltip("Screen pixels per background texture pixel while aiming. 1 = native resolution (100% zoom).")]
        [SerializeField, Min(0.1f)] float zoom = 1f;
        [Tooltip("Seconds to zoom in or out. 0 is instant.")]
        [SerializeField, Min(0f)] float zoomDuration = 0.15f;

        [Header("Sway")]
        [Tooltip("How far the aim drifts, in screen pixels. 0 turns the sway off.")]
        [SerializeField, Min(0f)] float swayPixels = 6f;
        [Tooltip("How fast the aim drifts. Higher is shakier.")]
        [SerializeField, Min(0f)] float swaySpeed = 0.3f;

        [SerializeField] InputAction aimAction = new("Aim", InputActionType.Button, "<Mouse>/rightButton");

        Camera cam;
        // 0 is the normal view, 1 is fully zoomed in.
        float zoomBlend;
        Vector2 lastCursor;

        /// <summary>Set by the hunt. Everywhere else the camera only frames the photo.</summary>
        public bool AllowAiming { get; set; }

        public bool IsAiming { get; private set; }

        /// <summary>Targets are only checked, and shots only allowed, once the zoom-in has finished.</summary>
        public bool IsFullyZoomed => IsAiming && zoomBlend >= 1f;

        /// <summary>World point at the center of the scope.</summary>
        public Vector2 AimPoint => cam.transform.position;

        public void SetBackground(SpriteRenderer value) => background = value;

        void Awake()
        {
            cam = GetComponent<Camera>();
            hud.SetActive(false);
        }

        void OnEnable() => aimAction.Enable();

        void OnDisable()
        {
            aimAction.Disable();
            SetAiming(false);
        }

        void LateUpdate()
        {
            bool aiming = AllowAiming && !PauseMenu.IsPaused && aimAction.IsPressed() && Pointer.current != null;
            if (aiming != IsAiming)
                SetAiming(aiming);
            if (IsAiming)
                lastCursor = Pointer.current.position.ReadValue();

            // Unscaled, so pausing while aiming still eases back to the full photo behind the menu.
            float step = zoomDuration > 0f ? Time.unscaledDeltaTime / zoomDuration : 1f;
            zoomBlend = Mathf.MoveTowards(zoomBlend, IsAiming ? 1f : 0f, step);

            Rect area = BackgroundRect();
            float fitSize = Mathf.Max(area.height, area.width / cam.aspect) * 0.5f;
            if (zoomBlend <= 0f)
            {
                cam.orthographicSize = fitSize;
                MoveTo(area.center);
                return;
            }

            float texelsPerUnit = background.sprite.rect.height / area.height;
            float aimSize = cam.pixelHeight / (2f * texelsPerUnit * zoom);
            Vector2 aimCenter = AimCenter(area, aimSize);
            if (zoomBlend >= 1f)
            {
                cam.orthographicSize = aimSize;
                MoveTo(SnapToPixels(aimCenter, area, aimSize));
                return;
            }

            float t = Mathf.SmoothStep(0f, 1f, zoomBlend);
            cam.orthographicSize = Mathf.Lerp(fitSize, aimSize, t);
            MoveTo(Vector2.Lerp(area.center, aimCenter, t));
        }

        void SetAiming(bool aiming)
        {
            IsAiming = aiming;
            Cursor.visible = !aiming;
            if (hud)
                hud.SetActive(aiming);
        }

        Vector2 AimCenter(Rect area, float size)
        {
            var normalized = new Vector2(
                Mathf.Clamp01(lastCursor.x / cam.pixelWidth),
                Mathf.Clamp01(lastCursor.y / cam.pixelHeight));
            float unitsPerPixel = 2f * size / cam.pixelHeight;
            float time = Time.time * swaySpeed;
            var sway = new Vector2(Mathf.PerlinNoise(time, 0f) - 0.5f, Mathf.PerlinNoise(0f, time + 37f) - 0.5f);
            return area.min + Vector2.Scale(normalized, area.size) + sway * (2f * swayPixels * unitsPerPixel);
        }

        // Snap the view to whole screen pixels from the background corner, so at integer zoom
        // each texel lands exactly on screen pixels instead of being filtered between two.
        Vector2 SnapToPixels(Vector2 center, Rect area, float size)
        {
            float unitsPerPixel = 2f * size / cam.pixelHeight;
            Vector2 halfView = new Vector2(cam.pixelWidth, cam.pixelHeight) * (0.5f * unitsPerPixel);
            Vector2 pixelsFromCorner = (center - halfView - area.min) / unitsPerPixel;
            Vector2 snappedCorner = area.min + new Vector2(Mathf.Round(pixelsFromCorner.x), Mathf.Round(pixelsFromCorner.y)) * unitsPerPixel;
            return snappedCorner + halfView;
        }

        void MoveTo(Vector2 point) => cam.transform.position = new Vector3(point.x, point.y, cam.transform.position.z);

        /// <summary>World rect of the background sprite. Works even while its GameObject is inactive.</summary>
        Rect BackgroundRect()
        {
            Bounds local = background.sprite.bounds;
            Vector2 min = background.transform.TransformPoint(local.min);
            Vector2 max = background.transform.TransformPoint(local.max);
            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }
    }
}
