using AutoSightseeingLog.Core.Localization;
using AutoSightseeingLog.Core.Tasks;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Glasswork.Backend;

namespace AutoSightseeingLog.Windows.Sections.Config;

internal static class GeneralSettings
{
    private const int ArrowSizeMinPercent = 50;
    private const int ArrowSizeMaxPercent = 200;
    private const string PercentFormat = "%d%%";
    private const string MinutesFormat = "%d min";
    private const float AppearanceWidth = 200f;
    private const string SliderValueId = "##value";

    // Ordered like SpotMarkerVisibility, because the enum value indexes these arrays.
    private static readonly LocString[] MarkerNames = [L.Settings.MarkerAlways, L.Settings.MarkerNear, L.Settings.MarkerAsked, L.Settings.MarkerNever];
    private static readonly LocString[] MarkerDetails = [L.Settings.MarkerAlwaysDetail, L.Settings.MarkerNearDetail, L.Settings.MarkerAskedDetail, L.Settings.MarkerNeverDetail];
    private static readonly string[] markerLabels = new string[MarkerNames.Length];

    private static readonly LocString[] AppearanceNames = [L.Settings.AppearanceDark, L.Settings.AppearanceLight];
    private static readonly string[] appearanceLabels = new string[AppearanceNames.Length];
    private static readonly string linkGlyph = FontAwesomeIcon.ExternalLinkAlt.ToIconString();

    private static SliderFormat windowWaitFormat;
    private static SliderFormat emotePauseFormat;
    private static SliderFormat arrowSizeFormat;

    public static void Draw(Configuration configuration)
    {
        DrawLanguageGroup(configuration);
        DrawThemeGroup(configuration);
        DrawWindowGroup(configuration);
        DrawBehaviorGroup(configuration);
        PostRunSettings.Draw(configuration);
        DrawMarkersGroup(configuration);
    }

    private static void DrawLanguageGroup(Configuration configuration)
    {
        if (Gw.BeginSection(Loc.T(L.Settings.Language)))
        {
            Gw.BeginRow(Loc.T(L.Settings.Language), default, "##asl_general_language");
            SettingsControls.DrawLanguageCombo(configuration);
            Gw.Tooltip(Loc.T(L.Settings.LanguageHelp));
            Gw.EndRow();
        }

        Gw.EndSection();
    }

    private static void DrawThemeGroup(Configuration configuration)
    {
        if (Gw.BeginSection(Loc.T(L.Settings.GroupTheme)))
        {
            Gw.BeginRow(Loc.T(L.Settings.Theme), default, "##asl_general_theme");
            DrawThemeCombo(configuration);
            Gw.Tooltip(Loc.T(L.Settings.ThemeHelp));
            Gw.EndRow();

            Gw.BeginRow(Loc.T(L.Settings.Appearance), default, "##asl_general_appearance");
            DrawAppearance(configuration);
            Gw.Tooltip(Loc.T(L.Settings.AppearanceHelp));
            Gw.EndRow();
        }

        Gw.EndSection();

        if (Gw.Button(Loc.T(L.Settings.ThemesBy), ButtonStyle.Plain, ButtonSize.Regular, linkGlyph))
        {
            UrlActions.OpenInBrowser(Themes.GlassworkUrl);
        }

        Gw.Tooltip(Themes.GlassworkUrl);
    }

    private static void DrawThemeCombo(Configuration configuration)
    {
        var selected = Themes.IndexOf(configuration.Theme);
        ImGui.SetNextItemWidth(SettingsControls.ComboWidth * ImGuiHelpers.GlobalScale);
        ImGui.PushID("asl_theme");
        var changed = Gw.ComboBox(ref selected, Themes.Names);
        ImGui.PopID();
        if (!changed)
        {
            return;
        }

        configuration.Theme = Themes.IdAt(selected);
        configuration.Save();
        Plugin.Instance.ApplyTheme();
    }

    private static void DrawAppearance(Configuration configuration)
    {
        SettingsControls.Translate(AppearanceNames, appearanceLabels);
        var selected = configuration.LightTheme ? 1 : 0;
        ImGui.SetNextItemWidth(AppearanceWidth * ImGuiHelpers.GlobalScale);
        if (!Gw.Segmented(ref selected, appearanceLabels))
        {
            return;
        }

        configuration.LightTheme = selected == 1;
        configuration.Save();
        Plugin.Instance.ApplyTheme();
    }

    private static void DrawWindowGroup(Configuration configuration)
    {
        if (Gw.BeginSection(Loc.T(L.Settings.GeneralWindow)))
        {
            var autoShow = configuration.AutoShowOnLogin;
            if (SettingsControls.ToggleRow(Loc.T(L.Settings.OpenOnLogin), Loc.T(L.Settings.OpenOnLoginHelp), "##asl_general_autoshow", ref autoShow))
            {
                configuration.AutoShowOnLogin = autoShow;
                configuration.SaveDebounced();
            }

            var dragAnywhere = configuration.DragWindowFromAnywhere;
            if (SettingsControls.ToggleRow(Loc.T(L.Settings.DragAnywhere), Loc.T(L.Settings.DragAnywhereHelp), "##asl_general_draganywhere", ref dragAnywhere))
            {
                configuration.DragWindowFromAnywhere = dragAnywhere;
                configuration.SaveDebounced();
            }
        }

        Gw.EndSection();
    }

    private static void DrawBehaviorGroup(Configuration configuration)
    {
        if (Gw.BeginSection(Loc.T(L.Settings.GeneralBehavior)))
        {
            var autoPause = configuration.AutoPauseInContent;
            if (SettingsControls.ToggleRow(Loc.T(L.Settings.AutoPause), Loc.T(L.Settings.AutoPauseHelp), "##asl_general_autopause", ref autoPause))
            {
                configuration.AutoPauseInContent = autoPause;
                configuration.SaveDebounced();
            }

            var askAtHardSpots = configuration.AskAtHardSpots;
            if (SettingsControls.ToggleRow(Loc.T(L.Settings.AskAtHardSpots), Loc.T(L.Settings.AskAtHardSpotsHelp), "##asl_general_askathardspots", ref askAtHardSpots))
            {
                configuration.AskAtHardSpots = askAtHardSpots;
                configuration.SaveDebounced();
            }

            var windowWait = configuration.WindowWaitMinutes;
            if (SettingsControls.SliderRow(Loc.T(L.Settings.WindowWait), Loc.T(L.Settings.WindowWaitHelp), "##asl_general_windowwait", ref windowWait,
                    AutoTour.MinWindowWaitMinutes, AutoTour.MaxWindowWaitMinutes, windowWaitFormat.For(MinutesFormat)))
            {
                configuration.WindowWaitMinutes = Math.Clamp(windowWait, AutoTour.MinWindowWaitMinutes, AutoTour.MaxWindowWaitMinutes);
                configuration.SaveDebounced();
            }

            DrawEmotePause(configuration);
        }

        Gw.EndSection();
    }

    private static void DrawEmotePause(Configuration configuration)
    {
        var shortest = configuration.EmotePauseMinSeconds;
        var longest = configuration.EmotePauseMaxSeconds;
        if (!SettingsControls.RangeRow(Loc.T(L.Settings.EmotePause), Loc.T(L.Settings.EmotePauseHelp), "##asl_general_emotepause", ref shortest, ref longest,
                0, EmotePause.MaxSeconds, emotePauseFormat.For(Loc.T(L.Settings.SecondsFormat))))
        {
            return;
        }

        configuration.EmotePauseMinSeconds = shortest;
        configuration.EmotePauseMaxSeconds = longest;
        configuration.SaveDebounced();
    }

    private static void DrawMarkersGroup(Configuration configuration)
    {
        if (Gw.BeginSection(Loc.T(L.Settings.Markers)))
        {
            SettingsControls.Translate(MarkerNames, markerLabels);

            var box = Math.Clamp((int)configuration.BoxVisibility, 0, MarkerNames.Length - 1);
            if (SettingsControls.ComboRow(Loc.T(L.Settings.MarkerBox), Loc.T(MarkerDetails[box]), Loc.T(L.Settings.MarkerBoxHelp), "##asl_markers_box", ref box, markerLabels))
            {
                configuration.BoxVisibility = (SpotMarkerVisibility)box;
                configuration.SaveDebounced();
            }

            var arrow = Math.Clamp((int)configuration.ArrowVisibility, 0, MarkerNames.Length - 1);
            if (SettingsControls.ComboRow(Loc.T(L.Settings.MarkerArrow), Loc.T(MarkerDetails[arrow]), Loc.T(L.Settings.MarkerArrowHelp), "##asl_markers_arrow", ref arrow, markerLabels))
            {
                configuration.ArrowVisibility = (SpotMarkerVisibility)arrow;
                configuration.SaveDebounced();
            }

            DrawArrowSize(configuration);
        }

        Gw.EndSection();
    }

    // A row of its own because the arrow previews above the character while its slider is hovered or dragged.
    private static void DrawArrowSize(Configuration configuration)
    {
        var size = configuration.ArrowSizePercent;
        Gw.BeginRow(Loc.T(L.Settings.MarkerArrowSize), default, "##asl_markers_arrowsize");
        ImGui.SetNextItemWidth(SettingsControls.SliderWidth * ImGuiHelpers.GlobalScale);
        var changed = Gw.Slider(ref size, ArrowSizeMinPercent, ArrowSizeMaxPercent, SliderValueId, 1, arrowSizeFormat.For(PercentFormat));
        if (ImGui.IsItemHovered() || ImGui.IsItemActive())
        {
            WorldOverlay.PreviewArrow();
        }

        Gw.Tooltip(Loc.T(L.Settings.MarkerArrowSizeHelp));
        Gw.EndRow();
        if (!changed)
        {
            return;
        }

        configuration.ArrowSizePercent = Math.Clamp(size, ArrowSizeMinPercent, ArrowSizeMaxPercent);
        configuration.SaveDebounced();
    }
}
