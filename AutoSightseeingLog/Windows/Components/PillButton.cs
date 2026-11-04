using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Glasswork.Backend;
using System.Numerics;

namespace AutoSightseeingLog.Windows.Components;

// A small Glasswork button whose width can be known before it is drawn, so pages can lay out rows of them.
internal static class PillButton
{
    public enum Emphasis { Ghost, Tinted, Filled }

    private const float DefaultHeight = 28f;

    // Glasswork measures with its own face; a few pixels of slack keep a label from touching the edge.
    private const float Slack = 4f;

    public static float Width(string label, FontAwesomeIcon? icon = null)
    {
        var look = Frame.Look;
        var scale = ImGuiHelpers.GlobalScale;
        var iconWidth = icon is { } glyph ? TextDraw.IconSize(glyph).X + look.SpaceSmall * scale : 0f;
        return (look.SpaceMedium * 2f + Slack) * scale + iconWidth + TextDraw.Measure(label).X;
    }

    // The button is centred on the slot the caller laid out, clicks only inside it, and takes only that room, so the
    // buttons and chips around it keep their own clicks and line up with it.
    public static bool Draw(string id, string label, Vector4 accent, Emphasis emphasis = Emphasis.Tinted,
        FontAwesomeIcon? icon = null, bool enabled = true, float height = DefaultHeight, string? tooltip = null)
    {
        var style = emphasis switch
        {
            Emphasis.Filled => ButtonStyle.Prominent,
            Emphasis.Tinted => ButtonStyle.Tinted,
            _ => ButtonStyle.Plain,
        };

        var scale = ImGuiHelpers.GlobalScale;
        var slot = new Vector2(Width(label, icon), height * scale);
        var origin = ImGui.GetCursorScreenPos();
        var clicked = false;
        ImGui.PushID(id);
        ImGui.BeginDisabled(!enabled);
        using (GlassworkSizes.FitHit(slot.Y))
        {
            GlassworkSizes.CenterInSlot(slot.Y, GlassworkSizes.HitHeight(ButtonSize.Small));
            var glyph = icon is { } value ? IconGlyphs.Of(value) : string.Empty;
            var tint = emphasis == Emphasis.Ghost ? (Glasswork.Color?)null : Styling.ToGlasswork(accent);
            clicked = Gw.Button(label, style, ButtonSize.Small, glyph, slot.X / scale, tint);
            if (tooltip is not null)
            {
                Gw.Tooltip(tooltip);
            }
        }

        ImGui.EndDisabled();
        ImGui.PopID();
        ImGui.SetCursorScreenPos(origin);
        ImGui.Dummy(slot);
        return clicked && enabled;
    }
}
