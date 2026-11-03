namespace AutoSightseeingLog.Core.Tasks;

internal static class EmotePause
{
    public const int MaxSeconds = 10;

    // The bounds are clamped and ordered here as well, because the configuration file can be edited by hand.
    public static int PickMilliseconds(int minSeconds, int maxSeconds, Random random)
    {
        var lowMs = Math.Clamp(minSeconds, 0, MaxSeconds) * TimeUnits.MillisecondsPerSecond;
        var highMs = Math.Clamp(maxSeconds, 0, MaxSeconds) * TimeUnits.MillisecondsPerSecond;
        if (highMs <= lowMs)
        {
            return lowMs;
        }

        return random.Next(lowMs, highMs + 1);
    }
}
