using AutoSightseeingLog.Core;
using AutoSightseeingLog.Core.External;
using AutoSightseeingLog.Core.Localization;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Glasswork.Backend;
using System.Numerics;

namespace AutoSightseeingLog.Windows.Shell;

internal static class NavRail
{
    private readonly record struct Entry(AppWindow.Page Page, string Glyph, LocString Label);

    private const float TopPad = 12f;
    private const float Gap = 8f;
    private const float BadgeOffset = 0.30f;
    private const float BadgeRadius = 3.5f;
    private const float BadgeRing = 1.5f;

    private static readonly Entry[] entries =
    [
        new(AppWindow.Page.Vistas,    FontAwesomeIcon.Binoculars.ToIconString(), L.Shell.NavVistas),
        new(AppWindow.Page.Settings,  FontAwesomeIcon.SlidersH.ToIconString(),   L.Shell.NavSettings),
        new(AppWindow.Page.History,   FontAwesomeIcon.ChartLine.ToIconString(),  L.Shell.NavHistory),
        new(AppWindow.Page.Plugins,   FontAwesomeIcon.Plug.ToIconString(),       L.Shell.NavPlugins),
        new(AppWindow.Page.Console,   FontAwesomeIcon.Terminal.ToIconString(),   L.Shell.NavConsole),
        new(AppWindow.Page.Changelog, FontAwesomeIcon.Newspaper.ToIconString(),  L.Shell.NavChangelog),
        new(AppWindow.Page.About,     FontAwesomeIcon.InfoCircle.ToIconString(), L.Shell.NavAbout),
    ];

    public static AppWindow.Page? Draw(AppWindow.Page current, Plugin plugin)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var button = GlassworkSizes.Height(ButtonSize.Large);
        var pitch = GlassworkSizes.HitHeight(ButtonSize.Large);
        var gap = Gap * scale;
        var railOrigin = ImGui.GetCursorScreenPos();
        var avail = ImGui.GetContentRegionAvail().X;
        var x = railOrigin.X + (avail - button) * 0.5f;
        var startY = railOrigin.Y + TopPad * scale;
        var dl = ImGui.GetWindowDrawList();
        var missingPlugins = !ExternalPlugins.AllRequiredInstalled();
        var running = plugin.Controller.Running;
        var unseenChangelog = plugin.Configuration.HasUnseenChangelog;
        AppWindow.Page? clicked = null;

        for (var index = 0; index < entries.Length; index++)
        {
            var entry = entries[index];
            var y = startY + (pitch + gap) * index;
            ImGui.SetCursorScreenPos(new Vector2(x, y));
            var style = entry.Page == current ? ButtonStyle.Tinted : ButtonStyle.Plain;
            if (Gw.IconButton(entry.Glyph, style, ButtonSize.Large, Loc.T(entry.Label)))
            {
                clicked = entry.Page;
            }

            DrawBadge(dl, entry.Page, current, new Vector2(x + button * 0.5f, y + pitch * 0.5f), button, missingPlugins, running, unseenChangelog);
        }

        ImGui.SetCursorScreenPos(railOrigin);
        ImGui.Dummy(new Vector2(avail, TopPad * scale + (pitch + gap) * entries.Length));
        return clicked;
    }

    private static void DrawBadge(ImDrawListPtr dl, AppWindow.Page page, AppWindow.Page current, Vector2 center, float button, bool missingPlugins, bool running, bool unseenChangelog)
    {
        Vector4 color;
        if (page == AppWindow.Page.Plugins && missingPlugins)
        {
            color = Styling.AccentRose;
        }
        else if (page == AppWindow.Page.Vistas && running)
        {
            color = Styling.PulseColor(Styling.AccentBlue, Styling.AccentBlueSoft, Styling.PulseMedium);
        }
        else if (page == AppWindow.Page.Changelog && current != AppWindow.Page.Changelog && unseenChangelog)
        {
            color = Styling.PulseColor(Styling.AccentStar, Styling.AccentStarSoft, Styling.PulseMedium);
        }
        else if (page == AppWindow.Page.Console && current != AppWindow.Page.Console && RunLog.Unseen is { } unseen)
        {
            color = unseen == RunLogLevel.Error ? Styling.AccentRose : Styling.AccentAmber;
        }
        else
        {
            return;
        }

        var scale = ImGuiHelpers.GlobalScale;
        var badgeCenter = center + new Vector2(button * BadgeOffset, -button * BadgeOffset);
        var radius = BadgeRadius * scale;
        dl.AddCircleFilled(badgeCenter, radius + BadgeRing * scale, Paint.Col(Styling.WindowBg with { W = 1f }));
        dl.AddCircleFilled(badgeCenter, radius, Paint.Col(color));
    }
}
