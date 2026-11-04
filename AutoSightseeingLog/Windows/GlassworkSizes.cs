using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Glasswork;
using Glasswork.Backend;

namespace AutoSightseeingLog.Windows;

// Glasswork sizes its controls in the Look's logical units; the window places some of them by hand and needs their
// size in pixels before they are drawn.
internal static class GlassworkSizes
{
    private static readonly TextStyle SelectedSegment = new(TextRole.Subheadline, FontWeight.Semibold);

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

    // Glasswork's segmented cells are all as wide as the widest one, so the control needs the widest label's room in
    // every cell.
    public static float SegmentedWidth(ReadOnlySpan<string> labels)
    {
        var look = Frame.Look;
        var scale = Frame.Scale;
        var widest = 0f;
        for (var index = 0; index < labels.Length; index++)
        {
            widest = MathF.Max(widest, TextPaint.Measure(labels[index], SelectedSegment, look, scale).X);
        }

        return (widest + look.SpaceLarge * 2f * scale) * labels.Length + look.SwitchKnobInset * 2f * scale;
    }
}
