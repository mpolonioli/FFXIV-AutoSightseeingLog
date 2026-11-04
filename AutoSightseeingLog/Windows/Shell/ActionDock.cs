using AutoSightseeingLog.Core.External;
using AutoSightseeingLog.Core.Localization;
using AutoSightseeingLog.Core.Tasks;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Glasswork;
using Glasswork.Backend;
using System.Numerics;

namespace AutoSightseeingLog.Windows.Shell;

internal static class ActionDock
{
    private const float PadX = 18f;
    private const float ButtonGap = 8f;
    private const float DetailInset = 22f;
    private const float DetailGap = 16f;
    private const float MinimumDetailWidth = 40f;
    private const float DetailAlpha = 0.8f;

    // Glasswork flips a filled button's ink from light to dark above this luminance.
    private const float InkFlipLuminance = 0.55f;

    private static readonly string startGlyph = FontAwesomeIcon.Play.ToIconString();
    private static readonly string stopGlyph = FontAwesomeIcon.Stop.ToIconString();
    private static readonly string pauseGlyph = FontAwesomeIcon.Pause.ToIconString();
    private static readonly string lockGlyph = FontAwesomeIcon.Lock.ToIconString();

    public static void Draw(Plugin plugin, Vector2 size, float windowRounding)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var origin = ImGui.GetCursorScreenPos();
        var drawList = ImGui.GetWindowDrawList();
        Dock.Background(drawList, origin, origin + size, windowRounding);

        var padX = PadX * scale;
        var buttonHeight = GlassworkSizes.HitHeight(ButtonSize.Large);
        var innerWidth = size.X - padX * 2f;
        ImGui.SetCursorScreenPos(new Vector2(origin.X + padX, origin.Y + (size.Y - buttonHeight) * 0.5f));

        if (plugin.Controller.Running)
        {
            DrawRunControls(plugin, innerWidth);
        }
        else
        {
            DrawStart(plugin, innerWidth);
        }

        ImGui.SetCursorScreenPos(origin);
        ImGui.Dummy(size);
    }

    private static void DrawRunControls(Plugin plugin, float innerWidth)
    {
        var controller = plugin.Controller;
        var gap = ButtonGap * ImGuiHelpers.GlobalScale;
        var half = (innerWidth - gap) * 0.5f;

        if (DrawPause(controller.PauseReason, half))
        {
            controller.TogglePause();
        }

        ImGui.SameLine(0f, gap);

        var session = controller.SessionSnapshot;
        var state = controller.Paused ? Loc.T(L.Tour.StatePaused) : Loc.T(L.Tour.StateRunning);
        var stopSub = session is null ? state : Loc.T(L.Tour.StopSub, state, Formatting.Elapsed(session.Elapsed));
        if (DrawHero(Loc.T(L.Tour.Stop), ButtonStyle.Destructive, null, stopGlyph, half, true, stopSub))
        {
            controller.Stop();
        }
    }

    private static bool DrawPause(PauseReason reason, float width)
    {
        var resume = Loc.T(L.Tour.ResumeCaps);
        return reason switch
        {
            PauseReason.InContent => DrawHero(resume, ButtonStyle.Prominent, Styling.ToGlasswork(Styling.AccentMint), lockGlyph, width, false, Loc.T(L.Tour.InContent)),
            PauseReason.Manual    => DrawHero(resume, ButtonStyle.Prominent, Styling.ToGlasswork(Styling.AccentMint), startGlyph, width, true, null),
            _                     => DrawHero(Loc.T(L.Tour.PauseCaps), ButtonStyle.Prominent, Styling.ToGlasswork(Styling.AccentAmber), pauseGlyph, width, true, null),
        };
    }

    private static void DrawStart(Plugin plugin, float innerWidth)
    {
        var configuration = plugin.Configuration;
        var plan = TourLauncher.Assess(configuration);
        var dependenciesReady = ExternalPlugins.AllRequiredInstalled();
        var canStart = plan.Readiness == TourLauncher.Readiness.Ready && dependenciesReady;
        var detail = canStart ? TourLauncher.Sublabel(configuration, plan) : !dependenciesReady ? Loc.T(L.Tour.ReasonInstall) : TourLauncher.Reason(plan);

        if (DrawHero(Loc.T(L.Tour.Start), ButtonStyle.Prominent, null, canStart ? startGlyph : lockGlyph, innerWidth, canStart, detail))
        {
            TourLauncher.Start();
        }
    }

    // A detail cut short by a narrow window shows in full as the button's tooltip.
    private static bool DrawHero(string label, ButtonStyle style, Color? tint, string glyph, float width, bool enabled, string? detail)
    {
        ImGui.BeginDisabled(!enabled);
        var clicked = Gw.Button(label, style, ButtonSize.Large, glyph, width / ImGuiHelpers.GlobalScale, tint);
        ImGui.EndDisabled();
        if (string.IsNullOrEmpty(detail))
        {
            return clicked && enabled;
        }

        // Drawn after the disabled scope and in the button's own ink, so a locked button's reason stays readable on its
        // faded fill.
        var ink = Styling.WithAlpha(Ink(style, tint), DetailAlpha);
        if (DrawDetail(detail, ink, ImGui.GetItemRectMin(), ImGui.GetItemRectMax(), ContentHalfWidth(label, glyph)))
        {
            Gw.Tooltip(detail);
        }

        return clicked && enabled;
    }

    private static Vector4 Ink(ButtonStyle style, Color? tint)
    {
        var look = Frame.Look;
        if (style == ButtonStyle.Destructive)
        {
            return InkOver(look.Destructive, look);
        }

        return tint is { } fill ? InkOver(fill, look) : look.OnAccent.ToVector4();
    }

    private static Vector4 InkOver(Color fill, Look look) => (fill.Luminance > InkFlipLuminance ? look.InkOverLight : look.InkOverDark).ToVector4();

    private static float ContentHalfWidth(string label, string glyph)
    {
        var look = Frame.Look;
        var scale = Frame.Scale;
        var labelWidth = TextPaint.Measure(label, TextRole.Headline, look, scale).X;
        var glyphWidth = TextPaint.Measure(glyph, TextRole.Icon, look, scale).X;
        return (labelWidth + glyphWidth + look.SpaceSmall * scale) * 0.5f;
    }

    // Returns true when the detail had to be cut short to stay clear of the button's centred label.
    private static bool DrawDetail(string detail, Vector4 ink, Vector2 min, Vector2 max, float contentHalfWidth)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var right = max.X - DetailInset * scale;
        var left = (min.X + max.X) * 0.5f + contentHalfWidth + DetailGap * scale;
        if (right - left < MinimumDetailWidth * scale)
        {
            return true;
        }

        var shown = TextDraw.Truncate(detail, right - left);
        var size = TextDraw.Measure(shown);
        TextDraw.At(shown, new Vector2(right - size.X, (min.Y + max.Y - size.Y) * 0.5f), ink);
        return !ReferenceEquals(shown, detail);
    }
}
