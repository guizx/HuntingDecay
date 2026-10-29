namespace HuntingDecay
{
    /// <summary>
    /// Anything inside a view that changes with the day or the Bloom. The ViewManager calls every
    /// listener in every view, active or not, whenever the state changes.
    /// </summary>
    public interface IDayStateListener
    {
        void OnDayState(int day, int missedAnomalies);
    }
}
