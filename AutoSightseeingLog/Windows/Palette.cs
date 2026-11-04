using Glasswork;
using System.Numerics;

namespace AutoSightseeingLog.Windows;

// The colours the window's own drawing uses. Grindless is the plugin's original set; any other theme is derived
// from its Glasswork Look, mixing the Look's background toward its text colour at the same steps the Grindless
// surfaces sit at, since Looks disagree on which of their background tokens is lighter.
internal sealed class Palette
{
    private const float SoftLift = 0.30f;
    private const float DeepDrop = 0.30f;
    private const float Surface0Step = 0.033f;
    private const float Surface1Step = 0.061f;
    private const float Surface2Step = 0.098f;
    private const float Surface3Step = 0.139f;
    private const float SliderStep = 0.118f;
    private const float BorderStep = 0.198f;

    // Over real glass the window keeps a light wash of its background and the cards let the glass show through.
    private const float SolidWindowAlpha = 0.985f;
    private const float GlassWindowAlpha = 0.35f;
    private const float SolidCardAlpha = 0.90f;
    private const float GlassCardAlpha = 0.55f;
    private const float SoftCardAlpha = 0.62f;
    private const float HoverCardAlpha = 0.95f;

    public Vector4 AccentStar;
    public Vector4 AccentStarSoft;
    public Vector4 AccentAether;
    public Vector4 AccentMint;
    public Vector4 AccentMintSoft;
    public Vector4 AccentAmber;
    public Vector4 AccentAmberSoft;
    public Vector4 AccentRose;
    public Vector4 AccentRoseSoft;
    public Vector4 AccentBlue;
    public Vector4 AccentBlueSoft;

    public Vector4 WindowBg;
    public Vector4 Surface0;
    public Vector4 Surface1;
    public Vector4 Surface2;
    public Vector4 Surface3;
    public Vector4 CardBg;
    public Vector4 CardBgSoft;
    public Vector4 CardBgHover;
    public Vector4 SliderBg;
    public Vector4 BorderDim;

    public Vector4 TextStrong;
    public Vector4 TextSecondary;
    public Vector4 TextDim;
    public Vector4 TextMuted;

    // What hairlines, borders and scrollbars are drawn in: white over a dark theme, the text colour over a light one.
    public Vector4 Ink;

    public bool IsLight;

    public static Palette Grindless() => new()
    {
        AccentStar = new(0.902f, 0.871f, 0.980f, 1.00f),
        AccentStarSoft = new(0.961f, 0.945f, 1.000f, 1.00f),
        AccentAether = new(0.851f, 0.353f, 0.651f, 1.00f),
        AccentMint = new(0.46f, 0.86f, 0.66f, 1.00f),
        AccentMintSoft = new(0.66f, 0.96f, 0.80f, 1.00f),
        AccentAmber = new(0.92f, 0.74f, 0.34f, 1.00f),
        AccentAmberSoft = new(1.00f, 0.86f, 0.52f, 1.00f),
        AccentRose = new(0.93f, 0.42f, 0.50f, 1.00f),
        AccentRoseSoft = new(1.00f, 0.62f, 0.68f, 1.00f),
        AccentBlue = new(0.40f, 0.68f, 0.98f, 1.00f),
        AccentBlueSoft = new(0.62f, 0.82f, 1.00f, 1.00f),

        WindowBg = new(0.047f, 0.039f, 0.086f, SolidWindowAlpha),
        Surface0 = new(0.075f, 0.063f, 0.122f, 1.00f),
        Surface1 = new(0.102f, 0.086f, 0.161f, 1.00f),
        Surface2 = new(0.133f, 0.114f, 0.208f, 1.00f),
        Surface3 = new(0.173f, 0.149f, 0.263f, 1.00f),
        CardBg = new(0.075f, 0.063f, 0.122f, SolidCardAlpha),
        CardBgSoft = new(0.102f, 0.086f, 0.161f, SoftCardAlpha),
        CardBgHover = new(0.133f, 0.114f, 0.208f, HoverCardAlpha),
        SliderBg = new(0.157f, 0.137f, 0.235f, 1.00f),
        BorderDim = new(0.239f, 0.208f, 0.353f, 1.00f),

        TextStrong = new(0.957f, 0.945f, 0.980f, 1.00f),
        TextSecondary = new(0.808f, 0.780f, 0.871f, 1.00f),
        TextDim = new(0.612f, 0.576f, 0.702f, 1.00f),
        TextMuted = new(0.431f, 0.400f, 0.529f, 1.00f),
        Ink = Vector4.One,
    };

    // The brand accent becomes the Look's accent and the second, deeper brand colour a darkened accent, so the pair
    // keeps its light-over-deep relationship in every theme; mint, amber, rose and blue take the status colours. The
    // soft variants are the ones text and highlights are drawn in, so they move away from the background: lighter on a
    // dark theme, darker on a light one.
    public static Palette From(Look look)
    {
        var light = look.IsLight;
        var background = look.Background.ToVector4() with { W = 1f };
        var label = look.Label.ToVector4();
        var glass = look.PanelMaterial.IsGlass;
        var cardAlpha = glass ? GlassCardAlpha : SolidCardAlpha;
        var accent = look.Accent.ToVector4();
        var success = look.Success.ToVector4();
        var warning = look.Warning.ToVector4();
        var destructive = look.Destructive.ToVector4();
        var info = look.Info.ToVector4();
        var surface0 = Mix(background, label, Surface0Step);
        var surface1 = Mix(background, label, Surface1Step);
        var surface2 = Mix(background, label, Surface2Step);
        var secondaryLabel = look.SecondaryLabel.ToVector4();

        return new Palette
        {
            AccentStar = accent,
            AccentStarSoft = Soft(accent, light),
            AccentAether = Styling.Darken(accent, DeepDrop),
            AccentMint = success,
            AccentMintSoft = Soft(success, light),
            AccentAmber = warning,
            AccentAmberSoft = Soft(warning, light),
            AccentRose = destructive,
            AccentRoseSoft = Soft(destructive, light),
            AccentBlue = info,
            AccentBlueSoft = Soft(info, light),

            WindowBg = background with { W = glass ? GlassWindowAlpha : SolidWindowAlpha },
            Surface0 = surface0,
            Surface1 = surface1,
            Surface2 = surface2,
            Surface3 = Mix(background, label, Surface3Step),
            CardBg = surface0 with { W = cardAlpha },
            CardBgSoft = surface1 with { W = SoftCardAlpha },
            CardBgHover = surface2 with { W = HoverCardAlpha },
            SliderBg = Mix(background, label, SliderStep),
            BorderDim = Mix(background, label, BorderStep),

            TextStrong = label,
            TextSecondary = Vector4.Lerp(label, secondaryLabel, 0.5f),
            TextDim = secondaryLabel,
            TextMuted = look.TertiaryLabel.ToVector4(),
            Ink = light ? label with { W = 1f } : Vector4.One,
            IsLight = light,
        };
    }

    private static Vector4 Soft(Vector4 color, bool light) => light ? Styling.Darken(color, SoftLift) : Styling.Lighten(color, SoftLift);

    private static Vector4 Mix(Vector4 from, Vector4 to, float amount) => Vector4.Lerp(from, to, amount) with { W = 1f };
}
