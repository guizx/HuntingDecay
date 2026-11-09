using System.Collections.Generic;
using UnityEngine;

namespace HuntingDecay
{
    public enum Weather { None, Leaves, Snow }

    /// <summary>
    /// One day of the story: what the Daughter says and how the day looks and sounds.
    /// Target placement lives in the views, not here.
    /// "Healthy" lines play with 0 missed anomalies, "Infected" lines with 1.
    /// </summary>
    [CreateAssetMenu(menuName = "Hunting Decay/Day", fileName = "Day")]
    public class DayData : ScriptableObject
    {
        [Header("Breakfast")]
        [Tooltip("Day 1 starts straight in the forest.")]
        public bool hasBreakfast = true;
        public Breakfast breakfastHealthy = new();
        public Breakfast breakfastInfected = new();
        public List<DialogueLine> huntRequest = new();

        [Header("Hunt")]
        [Tooltip("Shown in the scope's corner while aiming. Leave empty for none.")]
        [TextArea(1, 3)] public string scopeNote;
        [Tooltip("Whether the random forest sounds play during today's hunt.")]
        public bool randomSounds = true;

        [Header("Dinner")]
        public Dinner dinnerHealthy = new();
        public Dinner dinnerInfected = new();
        [Tooltip("Told after the reaction when the Bloom advanced today.")]
        public List<DialogueLine> missedNews = new();

        [Header("Atmosphere")]
        public Color colorFilter = Color.white;
        [Range(-100f, 100f)] public float temperature;
        [Range(-100f, 100f)] public float saturation;
        public float postExposure;
        [Tooltip("On: the forest photos use the Hunt values below instead of the grade above, "
            + "like keeping some color in the last hunt. The house and endings keep the grade above.")]
        public bool customHuntGrade;
        [Range(-100f, 100f)] public float huntTemperature;
        [Range(-100f, 100f)] public float huntSaturation;
        public float huntPostExposure;
        [Tooltip("The hue the photos' green foliage turns into, in degrees. 30 is orange.")]
        [Range(0f, 360f)] public float foliageHue = 30f;
        [Tooltip("How far the foliage moves toward Foliage Hue. 0 keeps its green.")]
        [Range(0f, 1f)] public float foliageHueAmount;
        [Tooltip("Multiplies the foliage's saturation after the shift. 0 is gray.")]
        [Range(0f, 2f)] public float foliageSaturation = 1f;
        [Tooltip("Pales the foliage toward white, like frost and snow. 0 is none.")]
        [Range(0f, 1f)] public float foliageWhiten;
        public Weather weather;
        [Min(0f)] public float weatherRate = 15f;
        [Tooltip("Plays only in the forest photos. The house's music depends on the Bloom, see the Story asset.")]
        public Sound music;
        [Tooltip("Plays only in the forest photos.")]
        public Sound forestAmbience;
    }
}
