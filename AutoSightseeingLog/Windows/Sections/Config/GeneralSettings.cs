using AutoSightseeingLog.Core.Localization;
using AutoSightseeingLog.Core.Tasks;
using AutoSightseeingLog.Windows.Components;
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

    // Ordered like SpotMarkerVisibility, because the enum value indexes this array.
    private static readonly SettingsControls.Choices.Choice[] markerVisibilityChoices =
    [
        new(L.Settings.MarkerAlways, L.Settings.MarkerAlwaysDetail),
        new(L.Settings.MarkerNear, L.Settings.MarkerNearDetail),
        new(L.Settings.MarkerAsked, L.Settings.MarkerAskedDetail),
        new(L.Settings.MarkerNever, L.Settings.MarkerNeverDetail),
    ];

    private static readonly LocString[] AppearanceNames = [L.Settings.AppearanceDark, L.Settings.AppearanceLight];
    private static readonly string[] appearanceLabels = new string[AppearanceNames.Length];
    private static readonly string linkGlyph = FontAwesomeIcon.ExternalLinkAlt.ToIconString();

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
        using var group = SettingsGroup.Begin(Loc.T(L.Settings.Language));

        SettingsRow.Draw(Loc.T(L.Settings.Language),
            Loc.T(L.Settings.LanguageHelp),
            SettingsControls.RowComboWidth,
            () => SettingsControls.DrawLanguageCombo(configuration));
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
        using var group = SettingsGroup.Begin(Loc.T(L.Settings.GeneralWindow));

        SettingsRow.Draw(Loc.T(L.Settings.OpenOnLogin),
            Loc.T(L.Settings.OpenOnLoginHelp),
            SettingsControls.ToggleWidth,
            () => SettingsControls.DrawToggle(configuration, () => configuration.AutoShowOnLogin, value => configuration.AutoShowOnLogin = value, "##asl_general_autoshow"),
            SettingsRow.ToggleHeight);

        SettingsRow.Draw(Loc.T(L.Settings.DragAnywhere),
            Loc.T(L.Settings.DragAnywhereHelp),
            SettingsControls.ToggleWidth,
            () => SettingsControls.DrawToggle(configuration, () => configuration.DragWindowFromAnywhere, value => configuration.DragWindowFromAnywhere = value, "##asl_general_draganywhere"),
            SettingsRow.ToggleHeight);
    }

    private static void DrawBehaviorGroup(Configuration configuration)
    {
        using var group = SettingsGroup.Begin(Loc.T(L.Settings.GeneralBehavior));

        SettingsRow.Draw(Loc.T(L.Settings.AutoPause),
            Loc.T(L.Settings.AutoPauseHelp),
            SettingsControls.ToggleWidth,
            () => SettingsControls.DrawToggle(configuration, () => configuration.AutoPauseInContent, value => configuration.AutoPauseInContent = value, "##asl_general_autopause"),
            SettingsRow.ToggleHeight);

        SettingsRow.Draw(Loc.T(L.Settings.AskAtHardSpots),
            Loc.T(L.Settings.AskAtHardSpotsHelp),
            SettingsControls.ToggleWidth,
            () => SettingsControls.DrawToggle(configuration, () => configuration.AskAtHardSpots, value => configuration.AskAtHardSpots = value, "##asl_general_askathardspots"),
            SettingsRow.ToggleHeight);

        SettingsRow.Draw(Loc.T(L.Settings.WindowWait),
            Loc.T(L.Settings.WindowWaitHelp),
            SettingsControls.RowSliderWidth,
            () => SettingsControls.DrawIntSlider(configuration, "##asl_general_windowwait", () => configuration.WindowWaitMinutes, value => configuration.WindowWaitMinutes = value,
                AutoTour.MinWindowWaitMinutes, AutoTour.MaxWindowWaitMinutes, MinutesFormat));

        SettingsRow.Draw(Loc.T(L.Settings.EmotePause),
            Loc.T(L.Settings.EmotePauseHelp),
            SettingsControls.RangeInlineWidth(),
            () => SettingsControls.DrawRangeInline(configuration, "##asl_general_emotepause_min", "##asl_general_emotepause_max",
                () => configuration.EmotePauseMinSeconds, value => configuration.EmotePauseMinSeconds = value,
                () => configuration.EmotePauseMaxSeconds, value => configuration.EmotePauseMaxSeconds = value,
                EmotePause.MaxSeconds, 0, Loc.T(L.Settings.SecondsFormat)));
    }

    private static void DrawMarkersGroup(Configuration configuration)
    {
        using var group = SettingsGroup.Begin(Loc.T(L.Settings.Markers));

        SettingsRow.Draw(Loc.T(L.Settings.MarkerBox),
            Loc.T(L.Settings.MarkerBoxHelp),
            SettingsControls.RowComboWidth,
            () => DrawVisibilityCombo(configuration, "##asl_markers_box", configuration.BoxVisibility, value => configuration.BoxVisibility = value));

        SettingsRow.Draw(Loc.T(L.Settings.MarkerArrow),
            Loc.T(L.Settings.MarkerArrowHelp),
            SettingsControls.RowComboWidth,
            () => DrawVisibilityCombo(configuration, "##asl_markers_arrow", configuration.ArrowVisibility, value => configuration.ArrowVisibility = value));

        SettingsRow.Draw(Loc.T(L.Settings.MarkerArrowSize),
            Loc.T(L.Settings.MarkerArrowSizeHelp),
            SettingsControls.RowSliderWidth,
            () => DrawArrowSizeSlider(configuration));
    }

    private static void DrawVisibilityCombo(Configuration configuration, string id, SpotMarkerVisibility current, Action<SpotMarkerVisibility> setter)
        => SettingsControls.Choices.DrawCombo(id, markerVisibilityChoices, (int)current, picked =>
        {
            setter((SpotMarkerVisibility)picked);
            configuration.SaveDebounced();
        });

    private static void DrawArrowSizeSlider(Configuration configuration)
    {
        SettingsControls.DrawIntSlider(configuration, "##asl_markers_arrowsize", () => configuration.ArrowSizePercent, value => configuration.ArrowSizePercent = value,
            ArrowSizeMinPercent, ArrowSizeMaxPercent, PercentFormat);
        if (ImGui.IsItemHovered() || ImGui.IsItemActive())
        {
            WorldOverlay.PreviewArrow();
        }
    }
}
