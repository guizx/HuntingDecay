namespace HuntingDecay
{
    /// <summary>Daughter isn't in the GDD's list. She's in the lake photo on the final day and can't be shot.</summary>
    public enum TargetType { Deer, Anomaly, Person, Decoy, Wolf, GiantPlant, Daughter }

    public enum Ending { None, HappyWinter, Withering, EndOfHollowPine }

    public readonly struct ShotResult
    {
        public readonly bool BloomAdvances;
        public readonly Ending Ending;

        public ShotResult(bool bloomAdvances, Ending ending)
        {
            BloomAdvances = bloomAdvances;
            Ending = ending;
        }
    }

    /// <summary>The shot table from the GDD. It's the same every day, so it lives in code, not in the day data.</summary>
    public static class ShotRules
    {
        public const int TutorialDay = 1;

        public static bool CanShoot(TargetType type, int day, bool isFinalDay) => type switch
        {
            TargetType.Daughter => false,
            // Day 1 is the tutorial, so it has no wrong choice. On the final day the choice is only
            // between the wolf and the plant.
            TargetType.Decoy => day > TutorialDay && !isFinalDay,
            _ => true,
        };

        public static ShotResult Resolve(TargetType hit, bool isFinalDay, bool anomalyPresent)
        {
            // Only the wolf or the plant can be shot on the final day.
            if (isFinalDay)
                return new ShotResult(false, hit == TargetType.GiantPlant ? Ending.HappyWinter : Ending.Withering);

            bool bloomAdvances = hit switch
            {
                TargetType.Anomaly => false,
                // The village loses its doctor.
                TargetType.Person => true,
                // A deer or a decoy: whatever anomaly was out there escapes.
                _ => anomalyPresent,
            };
            return new ShotResult(bloomAdvances, Ending.None);
        }
    }
}
