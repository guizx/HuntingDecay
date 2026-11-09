using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace HuntingDecay
{
    /// <summary>Everything in the story that isn't tied to one day.</summary>
    [CreateAssetMenu(menuName = "Hunting Decay/Story", fileName = "Story")]
    public class StoryData : ScriptableObject
    {
        [Tooltip("One asset per day, in order. The last one is the final day.")]
        public List<DayData> days = new();

        [Header("Last dinner (2 missed anomalies)")]
        public Dinner lastDinner = new();

        [Header("Endings")]
        public List<DialogueLine> happyWinter = new();
        [Tooltip("Happy Winter with 1 missed anomaly.")]
        public List<DialogueLine> happyWinterWithLoss = new();
        [FormerlySerializedAs("witheringWolf")]
        public List<DialogueLine> withering = new();
        public List<DialogueLine> endOfHollowPine = new();

        [Header("Title")]
        [Tooltip("The day whose grade and foliage colors the title screen uses.")]
        public DayData titleLook;
        public Sound titleMusic;

        [Header("Ending music")]
        [Tooltip("Both Happy Winter endings, after shooting the plant.")]
        public Sound plantEndingMusic;
        [Tooltip("Withering and The End of Hollow Pine.")]
        public Sound badEndingMusic;

        [Header("Interface")]
        public string dayCardFormat = "Day {0}";
        public string theEnd = "The End";
        public string shootPrompt = "[Left Click] Shoot";
        public string aimHint = "Hold Right Mouse to aim";
    }
}
