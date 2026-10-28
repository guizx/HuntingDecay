using UnityEngine;
using UnityEngine.InputSystem;

namespace HuntingDecay.Temp
{
    /// <summary>
    /// TEMPORARY test for the 32-color decision. Delete the whole _Temp folder once it's settled.
    /// 1: AllColors background with the dither shader. 2: baked 32-color background, shader off.
    /// </summary>
    public class TempBackgroundSwitch : MonoBehaviour
    {
        [SerializeField] GameObject allColorsBackground;
        [SerializeField] GameObject colors32Background;

        void Start() => Apply(useShader: true);

        void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
                return;
            if (keyboard.digit1Key.wasPressedThisFrame)
                Apply(useShader: true);
            else if (keyboard.digit2Key.wasPressedThisFrame)
                Apply(useShader: false);
        }

        // The global shader float outlives Play Mode in the Editor, so put it back.
        void OnDisable() => PaletteDither.SetEnabled(true);

        void Apply(bool useShader)
        {
            allColorsBackground.SetActive(useShader);
            colors32Background.SetActive(!useShader);
            PaletteDither.SetEnabled(useShader);
        }
    }
}
