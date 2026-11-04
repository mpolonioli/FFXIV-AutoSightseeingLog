using AutoSightseeingLog.Windows.Sections.Config;

namespace AutoSightseeingLog.Windows.Pages;

internal sealed class SettingsPage
{
    public void Draw(Plugin plugin) => GeneralSettings.Draw(plugin.Configuration);
}
