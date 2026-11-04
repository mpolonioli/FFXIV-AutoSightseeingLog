namespace AutoSightseeingLog.Core.Tasks;

internal static class EmotePause
{
    public const int MaxSeconds = 10;

    // The bounds are clamped and ordered here as well, because the configuration file can be edited by hand.
    public static (int LowSeconds, int HighSeconds) Bounds(int minSeconds, int maxSeconds)
    {
        var lowSeconds = Math.Clamp(minSeconds, 0, MaxSeconds);
        return (lowSeconds, Math.Clamp(maxSeconds, lowSeconds, MaxSeconds));
    }

    public static int PickMilliseconds(int minSeconds, int maxSeconds, Random random)
    {
        var (lowSeconds, highSeconds) = Bounds(minSeconds, maxSeconds);
        var lowMs = lowSeconds * TimeUnits.MillisecondsPerSecond;
        var highMs = highSeconds * TimeUnits.MillisecondsPerSecond;
        if (highMs == lowMs)
        {
            return lowMs;
        }

        return random.Next(lowMs, highMs + 1);
    }
}
