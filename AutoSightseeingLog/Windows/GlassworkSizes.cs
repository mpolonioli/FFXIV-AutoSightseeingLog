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

    // A control placed by hand in a slot shorter than the Look's minimum hit height would reach into the lines above and
    // below and take their clicks, so for the scope's life the minimum is lowered to the slot.
    public static HitFit FitHit(float slotHeight) => new(slotHeight / ImGuiHelpers.GlobalScale);

    public static void CenterInSlot(float slotHeight, float hitHeight)
    {
        var cursor = ImGui.GetCursorScreenPos();
        ImGui.SetCursorScreenPos(cursor with { Y = cursor.Y + (slotHeight - hitHeight) * 0.5f });
    }
}

// Restores the Look's minimum hit height when the hand-placed control it was lowered for is drawn.
internal readonly ref struct HitFit
{
    private readonly float savedMinimum;

    public HitFit(float slotUnits)
    {
        var look = Frame.Look;
        savedMinimum = look.MinHit;
        look.MinHit = MathF.Min(savedMinimum, slotUnits);
    }

    public void Dispose() => Frame.Look.MinHit = savedMinimum;
}
