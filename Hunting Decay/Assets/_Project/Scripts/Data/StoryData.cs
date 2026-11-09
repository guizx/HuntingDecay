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

        [Header("The Bloom spreading")]
        [Tooltip("Shown on black after a shot that lets an anomaly escape. A target can have its own card instead, "
            + "like shooting Dr. Marsh. On the second spread the jumpscare cuts in where the | is.")]
        public string missedAnomalyCard = "You missed an an|omaly.";
        [Tooltip("Cuts into the card on the second spread, right before the last dinner.")]
        public Jumpscare bloomJumpscare = new() { enabled = true };
        [Tooltip("Plays in the house, at breakfast and dinner, once an anomaly has been missed.")]
        public Sound infectedHouseMusic;
        [Tooltip("Plays as the first dinner after the first spread appears.")]
        public Sound firstInfectedDinnerSound;

        [Header("Endings")]
        public List<DialogueLine> happyWinter = new();
        [Tooltip("Happy Winter with 1 missed anomaly.")]
        public List<DialogueLine> happyWinterWithLoss = new();
        [FormerlySerializedAs("witheringWolf")]
        public List<DialogueLine> withering = new();
        public List<DialogueLine> endOfHollowPine = new();

        [Header("Ending cards")]
        public EndingCard endOfHollowPineCard = new() { name = "The End of Hollow Pine", number = 1 };
        public EndingCard witheringCard = new() { name = "Withering", number = 2 };
        [Tooltip("Happy Winter with 1 missed anomaly.")]
        public EndingCard bittersweetWinterCard = new() { good = true, name = "Bittersweet Winter", number = 3 };
        public EndingCard happyWinterCard = new() { good = true, name = "Happy Winter", number = 4 };
        public string badEndingLabel = "Bad Ending:";
        public string goodEndingLabel = "Good Ending:";
        [Tooltip("{0} is the label, {1} the name, {2} the number and {3} how many endings there are.")]
        [TextArea(3, 4)] public string endingCardFormat = "{0}\n{1}\n({2} of {3})";

        public int EndingCount => new[] { endOfHollowPineCard, witheringCard, bittersweetWinterCard, happyWinterCard }.Length;

        public string EndingCardText(EndingCard card) => string.Format(endingCardFormat,
            card.good ? goodEndingLabel : badEndingLabel, card.name, card.number, EndingCount);

        [Header("Title")]
        [Tooltip("The day whose grade and foliage colors the title screen uses.")]
        public DayData titleLook;
        public Sound titleMusic;

        [Header("Ending music")]
        [Tooltip("Both Happy Winter endings, after shooting the plant.")]
        public Sound plantEndingMusic;
        [Tooltip("Withering, and The End of Hollow Pine from its last dinner on.")]
        public Sound badEndingMusic;

        [Header("Interface")]
        public string dayCardFormat = "Day {0}";
        public string shootPrompt = "[Left Click] Shoot";
        public string aimHint = "Hold Right Mouse to aim";
    }
}
