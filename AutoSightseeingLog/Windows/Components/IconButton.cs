using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Glasswork.Backend;
using System.Numerics;

namespace AutoSightseeingLog.Windows.Components;

internal static class IconButton
{
    // The caller's size picks the nearest Glasswork button size, which is drawn centred on the caller's square.
    public static bool Draw(FontAwesomeIcon icon, string id, float size, Vector4? color = null, string? tooltip = null, bool enabled = true)
    {
        var buttonSize = size <= GlassworkSizes.Height(ButtonSize.Small) ? ButtonSize.Small
            : size <= GlassworkSizes.Height(ButtonSize.Regular) ? ButtonSize.Regular
            : ButtonSize.Large;

        ImGui.PushID(id);
        ImGui.BeginDisabled(!enabled);
        var cursor = ImGui.GetCursorScreenPos();
        ImGui.SetCursorScreenPos(cursor with { X = cursor.X + (size - GlassworkSizes.Height(buttonSize)) * 0.5f });
        GlassworkSizes.CenterInSlot(size, GlassworkSizes.HitHeight(buttonSize));
        var tint = color is { } value ? Styling.ToGlasswork(value) : (Glasswork.Color?)null;
        var clicked = Gw.IconButton(IconGlyphs.Of(icon), ButtonStyle.Plain, buttonSize, tooltip ?? string.Empty, tint);
        ImGui.EndDisabled();
        ImGui.PopID();
        return clicked && enabled;
    }
}
