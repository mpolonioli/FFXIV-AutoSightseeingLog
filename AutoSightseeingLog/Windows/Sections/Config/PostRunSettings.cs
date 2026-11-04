using AutoSightseeingLog.Core.Localization;
using Dalamud.Bindings.ImGui;
using Glasswork.Backend;

namespace AutoSightseeingLog.Windows.Sections.Config;

internal static class PostRunSettings
{
    private const int MaxLength = 1000;
    private const int MinLines = 3;
    private const int MaxLines = 8;

    public static void Draw(Configuration configuration)
    {
        if (Gw.BeginSection(Loc.T(L.Settings.PostRunGroup), Loc.T(L.Settings.PostRunCaption)))
        {
            SettingsControls.HeaderRow(Loc.T(L.Settings.PostRunLabel), "##asl_general_postrun_row");
            DrawEditor(configuration);
        }

        Gw.EndSection();
    }

    private static void DrawEditor(Configuration configuration)
    {
        var commands = configuration.PostRunCommands;
        var lineCount = 1;
        for (var charIndex = 0; charIndex < commands.Length; charIndex++)
        {
            if (commands[charIndex] == '\n')
            {
                lineCount++;
            }
        }

        ImGui.PushID("asl_general_postrun");
        var edited = Gw.TextArea(ref commands, default, Math.Clamp(lineCount + 1, MinLines, MaxLines), MaxLength);
        ImGui.PopID();

        // The help would cover the lines being typed, so it shows only while the editor is not focused.
        Gw.TooltipFor(ImGui.IsItemHovered() && !ImGui.IsItemActive(), Loc.T(L.Settings.PostRunHelp));
        if (!edited)
        {
            return;
        }

        configuration.PostRunCommands = commands;
        configuration.SaveDebounced();
    }
}
