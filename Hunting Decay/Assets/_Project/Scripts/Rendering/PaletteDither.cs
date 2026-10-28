using UnityEngine;

namespace HuntingDecay
{
    /// <summary>
    /// Runtime switch for the palette dither full-screen pass.
    /// Uses a global shader float instead of toggling the renderer feature, because SetActive on a
    /// feature edits the renderer asset and that change would survive leaving Play Mode.
    /// </summary>
    public static class PaletteDither
    {
        static readonly int OffId = Shader.PropertyToID("_PaletteDitherOff");

        public static void SetEnabled(bool enabled) => Shader.SetGlobalFloat(OffId, enabled ? 0f : 1f);
    }
}
