using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Glasswork.Backend;

namespace AutoSightseeingLog.Windows;

// Glasswork sizes its controls in the Look's logical units; the window places some of them by hand and needs their
// size in pixels before they are drawn.
internal static class GlassworkSizes
{
    public static float Height(ButtonSize size)
    {
        var look = Frame.Look;
        var height = size switch
        {
            ButtonSize.Small => look.ControlHeightSmall,
            ButtonSize.Large => look.ControlHeightLarge,
            _ => look.ControlHeight,
        };
        return height * ImGuiHelpers.GlobalScale;
    }

    // Every Glasswork control reserves at least the Look's minimum hit height and centres what it draws in it, so a
    // control placed by hand is centred on a line by this height, not by the height it is drawn at.
    public static float HitHeight(ButtonSize size) => MathF.Max(Height(size), Frame.Look.MinHit * ImGuiHelpers.GlobalScale);

    public static void CenterInSlot(float slotHeight, float hitHeight)
    {
        var cursor = ImGui.GetCursorScreenPos();
        ImGui.SetCursorScreenPos(cursor with { Y = cursor.Y + (slotHeight - hitHeight) * 0.5f });
    }
}
