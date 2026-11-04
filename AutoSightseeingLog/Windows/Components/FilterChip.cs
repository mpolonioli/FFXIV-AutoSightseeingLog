using Dalamud.Interface;
using System.Numerics;

namespace AutoSightseeingLog.Windows.Components;

// A filter that is on or off, drawn as a small Glasswork button tinted in its colour while it is on.
internal static class FilterChip
{
    private const string CountSeparator = "  ";
    private const int CacheSize = 16;

    private readonly record struct ChipText(string Label, string Count, string Text);

    private static readonly ChipText[] cache = new ChipText[CacheSize];
    private static int cacheNext;

    public static float Width(string label, string? count, FontAwesomeIcon? trailingIcon = null)
        => PillButton.Width(Text(label, count), trailingIcon);

    public static bool Draw(string id, string label, string? count, Vector4 accent, bool active, float height,
        FontAwesomeIcon? trailingIcon = null, string? tooltip = null)
        => PillButton.Draw(id, Text(label, count), accent, active ? PillButton.Emphasis.Tinted : PillButton.Emphasis.Ghost,
            trailingIcon, true, height, tooltip);

    // A count is usually the same from frame to frame, so the joined label is kept instead of built again.
    private static string Text(string label, string? count)
    {
        if (count is null)
        {
            return label;
        }

        for (var index = 0; index < cache.Length; index++)
        {
            var entry = cache[index];
            if (entry.Text is not null && ReferenceEquals(entry.Label, label) && string.Equals(entry.Count, count, StringComparison.Ordinal))
            {
                return entry.Text;
            }
        }

        var text = string.Concat(label, CountSeparator, count);
        cache[cacheNext] = new ChipText(label, count, text);
        cacheNext = (cacheNext + 1) % CacheSize;
        return text;
    }
}
