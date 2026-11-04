using AutoSightseeingLog.Core.Localization;
using AutoSightseeingLog.Windows.Sections.Config;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Glasswork.Backend;
using System.Numerics;

namespace AutoSightseeingLog.Windows.Pages;

internal sealed class SettingsPage
{
    private enum Tab { General }

    private const float TabGap = 16f;

    // Ordered like Tab, because the active tab indexes this array.
    private static readonly LocString[] TabNames = [L.Settings.CatGeneral];
    private static readonly string[] tabLabels = new string[TabNames.Length];

    private Tab activeTab = Tab.General;
    private bool resetScroll;

    public void Draw(Plugin plugin)
    {
        SettingsControls.Translate(TabNames, tabLabels);
        var selected = (int)activeTab;
        ImGui.SetNextItemWidth(SettingsControls.FullWidth);
        if (Gw.Segmented(ref selected, tabLabels) && selected != (int)activeTab)
        {
            activeTab = (Tab)selected;
            resetScroll = true;
        }

        Styling.VSpace(TabGap);
        using var content = ImRaii.Child("##asl_settings_content", new Vector2(-1f, -1f), false, ImGuiWindowFlags.None);
        if (!content)
        {
            return;
        }

        if (resetScroll)
        {
            ImGui.SetScrollY(0f);
            resetScroll = false;
        }

        var configuration = plugin.Configuration;
        using var reveal = Motion.PushSwitch("##asl_settings_tab", (int)activeTab);
        switch (activeTab)
        {
            case Tab.General:
                GeneralSettings.Draw(configuration);
                break;
        }
    }
}
