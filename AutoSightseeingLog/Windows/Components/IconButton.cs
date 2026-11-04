using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Glasswork.Backend;
using System.Numerics;

namespace AutoSightseeingLog.Windows.Components;

internal static class IconButton
{
    // The caller's size picks the nearest Glasswork button size, drawn centred on the caller's square; it clicks only
    // inside the square and takes only that room, so the rows and buttons around it keep their own clicks.
    public static bool Draw(FontAwesomeIcon icon, string id, float size, Vector4? color = null, string? tooltip = null, bool enabled = true)
    {
        var buttonSize = size <= GlassworkSizes.Height(ButtonSize.Small) ? ButtonSize.Small
            : size <= GlassworkSizes.Height(ButtonSize.Regular) ? ButtonSize.Regular
            : ButtonSize.Large;

        var origin = ImGui.GetCursorScreenPos();
        var clicked = false;
        ImGui.PushID(id);
        ImGui.BeginDisabled(!enabled);
        using (GlassworkSizes.FitHit(size))
        {
            ImGui.SetCursorScreenPos(origin with { X = origin.X + (size - GlassworkSizes.Height(buttonSize)) * 0.5f });
            GlassworkSizes.CenterInSlot(size, GlassworkSizes.HitHeight(buttonSize));
            var tint = color is { } value ? Styling.ToGlasswork(value) : (Glasswork.Color?)null;
            clicked = Gw.IconButton(IconGlyphs.Of(icon), ButtonStyle.Plain, buttonSize, tooltip ?? string.Empty, tint);
        }

        ImGui.EndDisabled();
        ImGui.PopID();
        ImGui.SetCursorScreenPos(origin);
        ImGui.Dummy(new Vector2(size, size));
        return clicked && enabled;
    }
}
