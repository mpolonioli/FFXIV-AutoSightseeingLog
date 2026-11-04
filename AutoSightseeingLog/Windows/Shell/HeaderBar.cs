using AutoSightseeingLog.Core.Localization;
using AutoSightseeingLog.Windows.Sections;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Glasswork;
using Glasswork.Backend;
using System.Numerics;

namespace AutoSightseeingLog.Windows.Shell;

internal static class HeaderBar
{
    private const string Title = "Auto Sightseeing Log";
    private const float PadX = 16f;
    private const float IconBox = 26f;
    private const float ButtonGap = 6f;
    private const int ButtonCount = 2;
    private const float CompactBarWidth = 90f;
    private const float MinimumPillRoom = 80f;

    // Room the window keeps for the bar's buttons when it is shrunk; Glasswork's buttons may differ slightly by theme.
    private const float ButtonAllowance = 30f;

    public const float MinimumWidth = PadX * 2f + IconBox + 12f + ButtonAllowance * ButtonCount + ButtonGap * (ButtonCount - 1);

    private static readonly string closeGlyph = FontAwesomeIcon.Times.ToIconString();
    private static readonly string minimizeGlyph = FontAwesomeIcon.ChevronDown.ToIconString();
    private static readonly string restoreGlyph = FontAwesomeIcon.ChevronUp.ToIconString();
    private static float pillHeight = 24f;

    public static float ButtonsWidth() => GlassworkSizes.Height(ButtonSize.Regular) * ButtonCount + ButtonGap * (ButtonCount - 1) * ImGuiHelpers.GlobalScale;

    public static bool HandleDrag(Vector2 windowPos, float width, float height)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var dragWidth = width - PadX * scale - ButtonsWidth() - 8f * scale;
        ImGui.SetCursorScreenPos(windowPos);
        ImGui.InvisibleButton("##asl_drag", new Vector2(MathF.Max(1f, dragWidth), height));
        var doubleClicked = ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left);
        if (!ImGui.IsItemActive())
        {
            return doubleClicked;
        }

        var delta = ImGui.GetIO().MouseDelta;
        if (delta != Vector2.Zero)
        {
            ImGui.SetWindowPos(ImGui.GetWindowPos() + delta, ImGuiCond.Always);
        }

        return doubleClicked;
    }

    public static void Draw(AppWindow window, Plugin plugin, Vector2 origin, float width, float height, float windowRounding, bool compact)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var drawList = ImGui.GetWindowDrawList();
        var end = origin + new Vector2(width, height);
        var padX = PadX * scale;
        var midY = origin.Y + height * 0.5f;

        Dock.Bar(drawList, origin, end, new Corners(windowRounding, windowRounding, compact ? windowRounding : 0f, compact ? windowRounding : 0f));
        if (!compact)
        {
            Paint.Hairline(drawList, new Vector2(origin.X, end.Y - 0.5f), new Vector2(end.X, end.Y - 0.5f));
        }

        var iconBox = IconBox * scale;
        var iconMin = new Vector2(origin.X + padX, midY - iconBox * 0.5f);
        AppIcon.Draw(drawList, iconMin, iconMin + new Vector2(iconBox, iconBox), 7f * scale);

        var buttonsLeft = end.X - padX - ButtonsWidth();
        var x = iconMin.X + iconBox + 12f * scale;
        using (Fonts.PushHeadline())
        {
            var titleSize = TextDraw.Measure(Title);
            if (x + titleSize.X <= buttonsLeft)
            {
                TextDraw.At(Title, new Vector2(x, midY - titleSize.Y * 0.5f), Styling.TextStrong);
                x += titleSize.X + 14f * scale;
            }
        }

        var info = ReadyState.Resolve(plugin.Configuration, plugin.Controller);
        var pillEnd = DrawStatusPill(info, x, buttonsLeft, midY);
        if (pillEnd > x)
        {
            x = pillEnd + 14f * scale;
        }

        if (compact)
        {
            DrawCompactInfo(plugin, info, x, buttonsLeft - 14f * scale, midY);
        }

        DrawButtons(window, end, midY, compact);
    }

    private static void DrawButtons(AppWindow window, Vector2 end, float midY, bool compact)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var padX = PadX * scale;
        var buttonSize = GlassworkSizes.Height(ButtonSize.Regular);
        var stride = buttonSize + ButtonGap * scale;
        var top = midY - GlassworkSizes.HitHeight(ButtonSize.Regular) * 0.5f;

        ImGui.SetCursorScreenPos(new Vector2(end.X - padX - buttonSize, top));
        if (Gw.IconButton(closeGlyph, ButtonStyle.Plain, ButtonSize.Regular, Loc.T(L.Common.Close)))
        {
            window.IsOpen = false;
        }

        ImGui.SetCursorScreenPos(new Vector2(end.X - padX - buttonSize - stride, top));
        if (Gw.IconButton(compact ? restoreGlyph : minimizeGlyph, ButtonStyle.Plain, ButtonSize.Regular,
                compact ? Loc.T(L.Shell.Restore) : Loc.T(L.Shell.Minimize)))
        {
            window.ToggleCompact();
        }
    }

    // The badge is centred on the bar from its height last frame; it cannot be measured before it is drawn.
    private static float DrawStatusPill(ReadyState.Info info, float x, float rightLimit, float midY)
    {
        if (rightLimit - x < MinimumPillRoom * ImGuiHelpers.GlobalScale)
        {
            return x;
        }

        ImGui.SetCursorScreenPos(new Vector2(x, midY - pillHeight * 0.5f));
        Gw.Badge(ReadyState.ShortLabel(info.Kind), Styling.ToGlasswork(info.Accent));
        pillHeight = ImGui.GetItemRectSize().Y;
        return ImGui.GetItemRectMax().X;
    }

    private static void DrawCompactInfo(Plugin plugin, ReadyState.Info info, float x, float rightX, float midY)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var controller = plugin.Controller;
        if (rightX - x < CompactBarWidth * scale)
        {
            return;
        }

        if (!controller.Running)
        {
            var configuration = plugin.Configuration;
            var plan = TourLauncher.Assess(configuration);
            if (plan.Readiness != TourLauncher.Readiness.Ready)
            {
                return;
            }

            var summary = TourLauncher.Sublabel(configuration, plan);
            using (Fonts.PushCaption())
            {
                var summarySize = TextDraw.Measure(summary);
                TextDraw.At(TextDraw.Truncate(summary, rightX - x), new Vector2(x, midY - summarySize.Y * 0.5f), Styling.TextDim);
            }

            return;
        }

        var session = controller.SessionSnapshot;
        var planned = session?.Plan.Count ?? 0;
        var logged = session?.VistasLogged ?? 0;
        var drawList = ImGui.GetWindowDrawList();
        var barWidth = CompactBarWidth * scale;
        var barX = rightX - barWidth;
        var barHeight = 6f * scale;
        var barOrigin = new Vector2(barX, midY - barHeight * 0.5f);
        if (planned > 0)
        {
            Paint.Bar(drawList, barOrigin, barWidth, barHeight, logged / (float)planned, info.Accent);
        }
        else if (controller.Paused)
        {
            Paint.Bar(drawList, barOrigin, barWidth, barHeight, 0f, info.Accent);
        }
        else
        {
            Paint.IndeterminateBar(drawList, barOrigin, barWidth, barHeight, info.Accent);
        }

        var textWidth = barX - 12f * scale - x;
        if (textWidth <= 0f)
        {
            return;
        }

        var phase = controller.Paused ? Loc.T(L.Run.PhasePaused) : ReadyState.PhaseLabel(controller.Phase);
        using (Fonts.PushCaption())
        {
            var phaseText = TextDraw.Truncate(phase, textWidth);
            var phaseSize = TextDraw.Measure(phaseText);
            var textY = midY - phaseSize.Y * 0.5f;
            TextDraw.At(phaseText, new Vector2(x, textY), Styling.TextSecondary);
            TextDraw.Trailing(controller.Status, x + phaseSize.X, x + textWidth, textY, Styling.TextSecondary);
        }
    }
}
