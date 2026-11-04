using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using System.Numerics;

namespace AutoSightseeingLog.Windows;

internal static class Styling
{
    private static Palette palette = Palette.Grindless();

    public static void Use(Palette next) => palette = next;

    public static Vector4 AccentStar => palette.AccentStar;
    public static Vector4 AccentStarSoft => palette.AccentStarSoft;
    public static Vector4 AccentAether => palette.AccentAether;
    public static readonly Vector4 InkOnStar        = new(0.078f, 0.059f, 0.133f, 1.00f);
    public static Vector4 AccentMint => palette.AccentMint;
    public static Vector4 AccentMintSoft => palette.AccentMintSoft;
    public static Vector4 AccentAmber => palette.AccentAmber;
    public static Vector4 AccentAmberSoft => palette.AccentAmberSoft;
    public static Vector4 AccentRose => palette.AccentRose;
    public static Vector4 AccentRoseSoft => palette.AccentRoseSoft;
    public static Vector4 AccentBlue => palette.AccentBlue;
    public static Vector4 AccentBlueSoft => palette.AccentBlueSoft;
    public static readonly Vector4 AccentDiscord    = new(0.345f, 0.396f, 0.949f, 1.00f);
    public static readonly Vector4 AccentPatreon    = new(1.000f, 0.259f, 0.302f, 1.00f);
    public static readonly Vector4 AccentPatreonSoft = new(1.000f, 0.580f, 0.600f, 1.00f);

    // The Patreon button's second gradient stop: the Grindless aether, kept fixed like the other brand colours.
    public static readonly Vector4 AccentPatreonDeep = new(0.851f, 0.353f, 0.651f, 1.00f);
    public static readonly Vector4 AccentCoffee     = new(1.000f, 0.867f, 0.000f, 1.00f);
    public static readonly Vector4 AccentCoffeeDeep = new(1.000f, 0.690f, 0.130f, 1.00f);
    public static readonly Vector4 InkOnCoffee      = new(0.130f, 0.090f, 0.040f, 1.00f);

    // Light ink for text on a dark fill under every theme: the Grindless text colour, since a light theme's own text
    // colour is dark.
    public static readonly Vector4 InkOnDark        = new(0.957f, 0.945f, 0.980f, 1.00f);

    public static Vector4 WindowBg => palette.WindowBg;
    public static Vector4 Surface0 => palette.Surface0;
    public static Vector4 Surface1 => palette.Surface1;
    public static Vector4 Surface2 => palette.Surface2;
    public static Vector4 Surface3 => palette.Surface3;

    public static Vector4 CardBg => palette.CardBg;
    public static Vector4 CardBgSoft => palette.CardBgSoft;
    public static Vector4 CardBgHover => palette.CardBgHover;
    public static Vector4 SliderBg => palette.SliderBg;
    public static Vector4 BorderDim => palette.BorderDim;

    public static Vector4 TextStrong => palette.TextStrong;
    public static Vector4 TextSecondary => palette.TextSecondary;
    public static Vector4 TextDim => palette.TextDim;
    public static Vector4 TextMuted => palette.TextMuted;

    public static Vector4 Hairline => palette.Ink with { W = 0.055f };
    public static Vector4 Highlight => palette.Ink with { W = 0.075f };

    public static Vector4 Ink(float alpha) => palette.Ink with { W = alpha };

    public const float WindowRounding = 14f;
    public const float PanelRounding = 12f;
    public const float CardRounding = 10f;
    public const float FrameRounding = 7f;

    public const double PulseFast = 600.0;
    public const double PulseMedium = 800.0;
    public const double PulseBreath = 2600.0;
    public const double PulseCalm = 1900.0;
    public const double PulseOrbit = 3400.0;

    public static float Pulse(double periodMs = PulseMedium)
    {
        var t = (Environment.TickCount % periodMs) / periodMs;
        return (float)((Math.Sin(t * Math.PI * 2.0) + 1.0) * 0.5);
    }

    public static Vector4 PulseColor(Vector4 a, Vector4 b, double periodMs = PulseMedium)
        => Vector4.Lerp(a, b, Pulse(periodMs));

    public static float Phase(double periodMs)
        => (float)((Environment.TickCount % periodMs) / periodMs);

    public static Vector4 WithAlpha(Vector4 c, float a) => c with { W = a };

    public static Vector4 Lighten(Vector4 c, float t) => Vector4.Lerp(c, Vector4.One, t) with { W = c.W };

    public static Vector4 Darken(Vector4 c, float t) => Vector4.Lerp(c, Vector4.Zero, t) with { W = c.W };

    // An accent pushed away from the window's background so text drawn in it stands out: lighter on a dark theme,
    // darker on a light one.
    public static Vector4 Emphasize(Vector4 c, float t) => palette.IsLight ? Darken(c, t) : Lighten(c, t);

    public static Glasswork.Color ToGlasswork(Vector4 color) => new(color.X, color.Y, color.Z, color.W);

    public static Vector4 Tint(Vector4 baseColor, Vector4 accent, float amount)
        => Vector4.Lerp(baseColor, accent, amount) with { W = baseColor.W };

    // Only the pale brand crosses 0.65; amber, mint and rose keep white text like the sibling plugins.
    public static Vector4 ForegroundOn(Vector4 fill) => Luminance(fill) > 0.65f ? InkOnStar : InkOnDark;

    // WCAG relative luminance of an sRGB color.
    private static float Luminance(Vector4 color) => 0.2126f * Linear(color.X) + 0.7152f * Linear(color.Y) + 0.0722f * Linear(color.Z);

    private static float Linear(float channel) => channel <= 0.04045f ? channel / 12.92f : MathF.Pow((channel + 0.055f) / 1.055f, 2.4f);

    public static void TextCentered(string text, Vector4 color)
    {
        var w = ImGui.CalcTextSize(text).X;
        var avail = ImGui.GetContentRegionAvail().X;
        if (avail > w) ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (avail - w) * 0.5f);
        using (ImRaii.PushColor(ImGuiCol.Text, color))
            ImGui.TextUnformatted(text);
    }

    public static void VSpace(float pixels)
        => ImGui.Dummy(new Vector2(0, pixels * ImGuiHelpers.GlobalScale));

    public static void CenterNextItem(float width)
    {
        var avail = ImGui.GetContentRegionAvail().X;
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + MathF.Max(0f, (avail - width) * 0.5f));
    }

    public static void SectionLabel(string label)
    {
        using (Fonts.PushHeadline())
        using (ImRaii.PushColor(ImGuiCol.Text, TextStrong))
            ImGui.TextUnformatted(label);
    }

    public static IDisposable PushChrome(Vector2 windowPadding)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var style = ImRaii.PushStyle(ImGuiStyleVar.WindowRounding, WindowRounding * scale)
            .Push(ImGuiStyleVar.WindowBorderSize, 1f)
            .Push(ImGuiStyleVar.WindowPadding, windowPadding * scale)
            .Push(ImGuiStyleVar.ChildRounding, CardRounding * scale)
            .Push(ImGuiStyleVar.ChildBorderSize, 0f)
            .Push(ImGuiStyleVar.PopupRounding, CardRounding * scale)
            .Push(ImGuiStyleVar.PopupBorderSize, 1f)
            .Push(ImGuiStyleVar.FrameRounding, FrameRounding * scale)
            .Push(ImGuiStyleVar.FramePadding, new Vector2(10f, 6f) * scale)
            .Push(ImGuiStyleVar.FrameBorderSize, 0f)
            .Push(ImGuiStyleVar.ItemSpacing, new Vector2(10f, 8f) * scale)
            .Push(ImGuiStyleVar.ItemInnerSpacing, new Vector2(6f, 4f) * scale)
            .Push(ImGuiStyleVar.ScrollbarSize, 9f * scale)
            .Push(ImGuiStyleVar.ScrollbarRounding, 9f * scale)
            .Push(ImGuiStyleVar.GrabRounding, 6f * scale)
            .Push(ImGuiStyleVar.GrabMinSize, 12f * scale);

        var color = ImRaii.PushColor(ImGuiCol.WindowBg, WindowBg)
            .Push(ImGuiCol.ChildBg, Vector4.Zero)
            .Push(ImGuiCol.PopupBg, Surface1 with { W = 0.985f })
            .Push(ImGuiCol.Border, Ink(0.09f))
            .Push(ImGuiCol.BorderShadow, Vector4.Zero)
            .Push(ImGuiCol.FrameBg, SliderBg)
            .Push(ImGuiCol.FrameBgHovered, Surface2)
            .Push(ImGuiCol.FrameBgActive, Surface3)
            .Push(ImGuiCol.ScrollbarBg, Vector4.Zero)
            .Push(ImGuiCol.ScrollbarGrab, Ink(0.12f))
            .Push(ImGuiCol.ScrollbarGrabHovered, Ink(0.20f))
            .Push(ImGuiCol.ScrollbarGrabActive, Ink(0.28f))
            .Push(ImGuiCol.Button, Surface1)
            .Push(ImGuiCol.ButtonHovered, Surface2)
            .Push(ImGuiCol.ButtonActive, Tint(Surface2, AccentStar, 0.35f))
            .Push(ImGuiCol.Header, Tint(Surface1, AccentStar, 0.30f))
            .Push(ImGuiCol.HeaderHovered, Surface2)
            .Push(ImGuiCol.HeaderActive, Tint(Surface2, AccentStar, 0.40f))
            .Push(ImGuiCol.CheckMark, AccentStarSoft)
            .Push(ImGuiCol.SliderGrab, AccentStar)
            .Push(ImGuiCol.SliderGrabActive, AccentStarSoft)
            .Push(ImGuiCol.Text, TextStrong)
            .Push(ImGuiCol.TextDisabled, TextMuted)
            .Push(ImGuiCol.Separator, Hairline)
            .Push(ImGuiCol.ResizeGrip, Vector4.Zero)
            .Push(ImGuiCol.ResizeGripHovered, Vector4.Zero)
            .Push(ImGuiCol.ResizeGripActive, Vector4.Zero)
            .Push(ImGuiCol.TextSelectedBg, WithAlpha(AccentAether, 0.40f));

        return new ChromeScope(style, color);
    }

    private sealed class ChromeScope(IDisposable style, IDisposable color) : IDisposable
    {
        public void Dispose()
        {
            color.Dispose();
            style.Dispose();
        }
    }
}
