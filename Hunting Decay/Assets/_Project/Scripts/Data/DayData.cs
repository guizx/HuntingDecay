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
        public Weather weather;
        [Min(0f)] public float weatherRate = 15f;
        public Sound music;
        [Tooltip("Plays only in the forest photos.")]
        public Sound forestAmbience;
    }
}
