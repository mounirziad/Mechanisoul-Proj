public interface ISlowable
{
    /// <param name="percent">0..1 (e.g., 0.3 = 30% slow)</param>
    /// <param name="seconds">duration of the slow</param>
    void AddSlow(float percent, float seconds);
}
