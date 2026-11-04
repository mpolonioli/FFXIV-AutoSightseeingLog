using AutoSightseeingLog.Core.Localization;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Glasswork;
using Glasswork.Backend;

namespace AutoSightseeingLog.Windows.Sections.Config;

internal static class SettingsControls
{
    public const float ComboWidth = 200f;
    public const float SliderWidth = 180f;
    public const float FullWidth = -float.Epsilon;

    private const float RangeSliderWidth = 130f;
    private const string ValueId = "##value";
    private const string RangeMinimumId = "##minimum";
    private const string RangeMaximumId = "##maximum";
    private const string RangeDash = "-";

    private static readonly string[] languageLabels = BuildLanguageLabels();

    // A Glasswork row with a switch; the explanation is the switch's tooltip. True on the frame it flips.
    public static bool ToggleRow(string title, string help, string id, ref bool value)
    {
        Gw.BeginRow(title, default, id);
        var changed = Gw.Toggle(ref value, id);
        Gw.Tooltip(help);
        Gw.EndRow();
        return changed;
    }

    public static bool SliderRow(string title, string help, string id, ref int value, int minimum, int maximum, string format)
    {
        Gw.BeginRow(title, default, id);
        ImGui.SetNextItemWidth(SliderWidth * ImGuiHelpers.GlobalScale);
        var changed = Gw.Slider(ref value, minimum, maximum, ValueId, 1, format);
        Gw.Tooltip(help);
        Gw.EndRow();
        return changed;
    }

    // The shortest and longest value of a range, side by side; pushing one past the other carries the other along.
    public static bool RangeRow(string title, string help, string id, ref int shortest, ref int longest, int minimum, int maximum, string format)
    {
        var width = RangeSliderWidth * ImGuiHelpers.GlobalScale;
        Gw.BeginRow(title, default, id);
        var lineTop = ImGui.GetCursorPosY();
        ImGui.SetNextItemWidth(width);
        var shortestChanged = Gw.Slider(ref shortest, minimum, maximum, RangeMinimumId, 1, format);
        Gw.Tooltip(help);
        var sliderHeight = ImGui.GetItemRectSize().Y;
        ImGui.SameLine();
        ImGui.SetCursorPosY(lineTop + (sliderHeight - TextPaint.LineHeight(TextRole.Body, Frame.Look, Frame.Scale)) * 0.5f);
        Gw.Text(RangeDash, TextRole.Body, Frame.Look.SecondaryLabel);
        ImGui.SameLine();
        ImGui.SetCursorPosY(lineTop);
        ImGui.SetNextItemWidth(width);
        var longestChanged = Gw.Slider(ref longest, minimum, maximum, RangeMaximumId, 1, format);
        Gw.Tooltip(help);
        Gw.EndRow();

        if (shortestChanged && shortest > longest)
        {
            longest = shortest;
        }

        if (longestChanged && longest < shortest)
        {
            shortest = longest;
        }

        return shortestChanged || longestChanged;
    }

    public static bool ComboRow(string title, string subtitle, string help, string id, ref int selected, ReadOnlySpan<string> labels)
    {
        Gw.BeginRow(title, subtitle, id);
        ImGui.SetNextItemWidth(ComboWidth * ImGuiHelpers.GlobalScale);
        ImGui.PushID(id);
        var changed = Gw.ComboBox(ref selected, labels);
        ImGui.PopID();
        Gw.Tooltip(help);
        Gw.EndRow();
        return changed;
    }

    public static void HeaderRow(string title, string id)
    {
        Gw.BeginRow(title, default, id);
        Gw.EndRow();
    }

    // The catalog hands back cached strings, so refilling a label buffer allocates nothing per frame.
    public static void Translate(LocString[] names, string[] labels)
    {
        for (var index = 0; index < names.Length; index++)
        {
            labels[index] = Loc.T(names[index]);
        }
    }

    public static void DrawLanguageCombo(Configuration configuration)
    {
        var languages = Languages.All;
        var selected = 0;
        for (var index = 0; index < languages.Length; index++)
        {
            if (ReferenceEquals(languages[index], Loc.Current))
            {
                selected = index;
            }
        }

        ImGui.SetNextItemWidth(ComboWidth * ImGuiHelpers.GlobalScale);
        ImGui.PushID("asl_language");
        var changed = Gw.ComboBox(ref selected, languageLabels);
        ImGui.PopID();
        if (!changed)
        {
            return;
        }

        configuration.Language = languages[selected].Code;
        configuration.Save();
        Loc.SetLanguage(configuration.Language);
        Fonts.OnLanguageChanged();
        Plugin.Instance.OnLanguageChanged();
    }

    private static string[] BuildLanguageLabels()
    {
        var languages = Languages.All;
        var labels = new string[languages.Length];
        for (var index = 0; index < languages.Length; index++)
        {
            labels[index] = languages[index].NativeName;
        }

        return labels;
    }
}
