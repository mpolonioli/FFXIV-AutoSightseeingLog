using Dalamud.Interface;

namespace AutoSightseeingLog.Windows;

// Glasswork takes an icon as a string; each glyph's string is built once instead of on every frame it is drawn.
internal static class IconGlyphs
{
    private static readonly Dictionary<FontAwesomeIcon, string> glyphs = new();

    public static string Of(FontAwesomeIcon icon)
    {
        if (glyphs.TryGetValue(icon, out var glyph))
        {
            return glyph;
        }

        glyph = icon.ToIconString();
        glyphs[icon] = glyph;
        return glyph;
    }
}
