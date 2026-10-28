using UnityEngine;
using UnityEngine.InputSystem;

namespace HuntingDecay
{
    /// <summary>
    /// Rifle scope. While aim is held, shows the HUD and zooms the camera so the background is drawn at
    /// <see cref="zoom"/> screen pixels per texture pixel. The cursor's normalized screen position picks
    /// the same normalized point of the background, which is placed at the camera center (the aim point).
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class Scope : MonoBehaviour
    {
        [SerializeField] SpriteRenderer background;
        [SerializeField] GameObject hud;

        [Tooltip("Screen pixels per background texture pixel while aiming. 1 = native resolution (100% zoom).")]
        [SerializeField, Min(0.1f)] float zoom = 1f;

        [SerializeField] InputAction aimAction = new InputAction("Aim", InputActionType.Button, "<Mouse>/rightButton");

        Camera cam;

        public bool IsAiming { get; private set; }

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
            bool aiming = aimAction.IsPressed() && Pointer.current != null;
            if (aiming != IsAiming)
                SetAiming(aiming);

            Rect area = BackgroundRect();
            if (IsAiming)
                AimAt(area, Pointer.current.position.ReadValue());
            else
                FitToScreen(area);
        }

        void SetAiming(bool aiming)
        {
            IsAiming = aiming;
            Cursor.visible = !aiming;
            if (hud)
                hud.SetActive(aiming);
        }

        /// <summary>Normal view: the background covers the whole screen.</summary>
        void FitToScreen(Rect area)
        {
            cam.orthographicSize = Mathf.Max(area.height, area.width / cam.aspect) * 0.5f;
            MoveTo(area.center);
        }

        void AimAt(Rect area, Vector2 cursor)
        {
            float texelsPerUnit = background.sprite.rect.height / area.height;
            cam.orthographicSize = cam.pixelHeight / (2f * texelsPerUnit * zoom);

            var normalized = new Vector2(
                Mathf.Clamp01(cursor.x / cam.pixelWidth),
                Mathf.Clamp01(cursor.y / cam.pixelHeight));
            Vector2 center = area.min + Vector2.Scale(normalized, area.size);

            // Snap the view to whole screen pixels from the background corner, so at integer zoom
            // each texel lands exactly on screen pixels instead of being filtered between two.
            float unitsPerPixel = 2f * cam.orthographicSize / cam.pixelHeight;
            Vector2 halfView = new Vector2(cam.pixelWidth, cam.pixelHeight) * (0.5f * unitsPerPixel);
            Vector2 pixelsFromCorner = (center - halfView - area.min) / unitsPerPixel;
            Vector2 snappedCorner = area.min + new Vector2(Mathf.Round(pixelsFromCorner.x), Mathf.Round(pixelsFromCorner.y)) * unitsPerPixel;
            MoveTo(snappedCorner + halfView);
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
