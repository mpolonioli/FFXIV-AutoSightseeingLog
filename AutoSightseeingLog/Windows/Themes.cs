using AutoSightseeingLog.Core;
using Glasswork;
using Glasswork.Dalamud;

namespace AutoSightseeingLog.Windows;

internal readonly record struct ThemeChoice(string Id, string Name, Func<Look> Dark, Func<Look> Light);

// The themes the whole window can wear: the plugin's own Grindless colours, then every Look family Glasswork
// ships, each in its dark and light appearance.
internal static class Themes
{
    public const string DefaultId = "grindless";
    public const string GlassworkUrl = "https://github.com/XeldarAlz/FFXIV-Glasswork";

    private const float RadiusSmall = 7f;
    private const float RadiusMedium = 10f;
    private const float RadiusLarge = 14f;
    private const float TextScaleTolerance = 0.001f;

    private static readonly ThemeChoice[] choices =
    [
        new(DefaultId, "Grindless", GrindlessDark, GrindlessLight),
        new("neutral", "Neutral", Look.NeutralDark, Look.NeutralLight),
        new("liquid", "Liquid", Look.Liquid, Look.LiquidLight),
        new("eorzea", "Eorzea", Look.Eorzea, Look.EorzeaLight),
        new("classic", "Classic", Look.Classic, Look.ClassicLight),
        new("clearBlue", "Clear Blue", Look.ClearBlue, Look.ClearBlueLight),
        new("clearGrey", "Clear Grey", Look.ClearGrey, Look.ClearGreyLight),
        new("clearGreen", "Clear Green", Look.ClearGreen, Look.ClearGreenLight),
        new("clearPink", "Clear Pink", Look.ClearPink, Look.ClearPinkLight),
        new("aurora", "Aurora", Look.Aurora, Look.AuroraLight),
        new("catppuccin", "Catppuccin", Look.Catppuccin, Look.CatppuccinLight),
        new("nord", "Nord", Look.Nord, Look.NordLight),
        new("minimal", "Minimal", Look.Minimal, Look.MinimalLight),
        new("retro", "Retro", Look.Retro, Look.RetroLight),
        new("highContrast", "High contrast", Look.HighContrast, Look.HighContrastLight),
    ];

    private static readonly string[] names = BuildNames();

    public static ReadOnlySpan<string> Names => names;

    public static bool UsesGlass { get; private set; }

    public static float GlassAmount { get; private set; }

    public static bool IsLight { get; private set; }

    public static int IndexOf(string id)
    {
        for (var index = 0; index < choices.Length; index++)
        {
            if (string.Equals(choices[index].Id, id, StringComparison.Ordinal))
            {
                return index;
            }
        }

        return 0;
    }

    public static string IdAt(int index) => choices[Math.Clamp(index, 0, choices.Length - 1)].Id;

    public static void Apply(GlassworkHost host, string id, bool light)
    {
        var choice = choices[IndexOf(id)];
        var look = light ? choice.Light() : choice.Dark();
        MatchControlText(look.Type);
        KeepOffMonoFace(look.Type, choice.Name);
        host.Look = look;
        UsesGlass = look.PanelMaterial.IsGlass;
        GlassAmount = look.GlassAmount;
        IsLight = look.IsLight;
        Styling.Use(choice.Id == DefaultId && !light ? Palette.Grindless() : Palette.From(look));
    }

    // Glasswork sizes its text from a fixed type scale while the window's own text follows Dalamud's font size, so
    // the scale is stretched until Glasswork's body text matches the window's, whatever size the player picked.
    public static void SyncTextScale(GlassworkHost host)
    {
        var type = host.Look.Type;
        var scale = Fonts.BodyPixels / type.Size(TextRole.Body);
        if (MathF.Abs(type.TextScale - scale) > TextScaleTolerance)
        {
            type.TextScale = scale;
        }
    }

    // Glasswork writes control labels in Subheadline and tooltips in Footnote, a step or two under its body text, while
    // the window's own controls and tooltips use its body size; both roles are raised to the body size to match.
    private static void MatchControlText(TypeScale type)
    {
        var body = type.Size(TextRole.Body);
        type.Set(TextRole.Subheadline, body, type.Weight(TextRole.Subheadline));
        type.Set(TextRole.Footnote, body, type.Weight(TextRole.Footnote));
    }

    // Glasswork.Dalamud 0.1.0-beta.3 builds its monospace face by reading the font file through a pointer to a stack
    // copy that is gone by then, which crashed the game whenever Retro, the Look that sends every label to that face,
    // drew a window. Every role that would draw in it keeps the text face until the package carries the fix.
    private static void KeepOffMonoFace(TypeScale type, string themeName)
    {
        var moved = 0;
        for (var role = TextRole.LargeTitle; role <= TextRole.Icon; role++)
        {
            if (type.Face(role) != TextRole.Mono)
            {
                continue;
            }

            type.SetFace(role, role == TextRole.Mono ? TextRole.Body : role);
            moved++;
        }

        RunLog.Debug($"Theme {themeName}: {moved} text roles moved off the monospace face, which this Glasswork version cannot build safely.");
    }

    // Neutral's controls repainted with the Grindless colours and corners, so Glasswork's widgets sit in the
    // window exactly like the plugin's own.
    private static Look GrindlessDark()
    {
        var palette = Palette.Grindless();
        var look = WithGrindlessAccents(Look.NeutralDark(), palette);
        look.Background = Styling.ToGlasswork(palette.WindowBg with { W = 1f });
        look.SecondaryBackground = Styling.ToGlasswork(palette.Surface0);
        look.TertiaryBackground = Styling.ToGlasswork(palette.Surface1);
        look.Label = Styling.ToGlasswork(palette.TextStrong);
        look.SecondaryLabel = Styling.ToGlasswork(palette.TextDim);
        look.TertiaryLabel = Styling.ToGlasswork(palette.TextMuted);

        // The pale brand accent only reads under the plugin's dark ink, and Glasswork paints switch and slider knobs in
        // that ink too, so an off switch gets a track light enough to show a dark knob.
        look.OnAccent = Styling.ToGlasswork(Styling.InkOnStar);
        look.ToggleOff = Styling.ToGlasswork(palette.TextMuted);
        return look;
    }

    private static Look GrindlessLight() => WithGrindlessAccents(Look.NeutralLight(), Palette.Grindless());

    // WithAccent also sets ToggleOn to the accent, nudged darker on a light background so the pale brand stays visible.
    private static Look WithGrindlessAccents(Look baseLook, Palette palette)
    {
        var look = baseLook
            .WithAccent(Styling.ToGlasswork(palette.AccentStar))
            .WithStatusColors(Styling.ToGlasswork(palette.AccentMint), Styling.ToGlasswork(palette.AccentAmber), Styling.ToGlasswork(palette.AccentRose), Styling.ToGlasswork(palette.AccentBlue));
        look.Name = "Grindless";
        look.RadiusSmall = RadiusSmall;
        look.RadiusMedium = RadiusMedium;
        look.RadiusLarge = RadiusLarge;
        return look;
    }

    private static string[] BuildNames()
    {
        var built = new string[choices.Length];
        for (var index = 0; index < choices.Length; index++)
        {
            built[index] = choices[index].Name;
        }

        return built;
    }
}
