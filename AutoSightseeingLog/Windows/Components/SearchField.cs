using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Glasswork.Backend;
using System.Numerics;

namespace AutoSightseeingLog.Windows.Components;

internal static class SearchField
{
    public const int DefaultMaxLength = 96;
    public const float DefaultHeight = 36f;

    private const float ClearGap = 4f;

    private static readonly string searchGlyph = FontAwesomeIcon.Search.ToIconString();
    private static readonly string clearGlyph = FontAwesomeIcon.Times.ToIconString();

    public static bool Draw(string id, string hint, ref string text, ref bool focused, float width, float height = DefaultHeight,
        bool requestFocus = false, int maxLength = DefaultMaxLength)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var slot = height * scale;
        var hasText = text.Length > 0;
        var clearSize = GlassworkSizes.Height(ButtonSize.Small);
        var clearRoom = hasText ? clearSize + ClearGap * scale : 0f;
        var origin = ImGui.GetCursorScreenPos();

        ImGui.PushID(id);
        GlassworkSizes.CenterInSlot(slot, GlassworkSizes.HitHeight(ButtonSize.Regular));
        if (requestFocus)
        {
            ImGui.SetKeyboardFocusHere();
        }

        ImGui.SetNextItemWidth(MathF.Max(1f, width - clearRoom));
        var changed = Gw.TextField(ref text, hint, default, maxLength, false, searchGlyph);
        focused = ImGui.IsItemActive();
        if (hasText)
        {
            ImGui.SetCursorScreenPos(new Vector2(origin.X + width - clearSize, origin.Y));
            GlassworkSizes.CenterInSlot(slot, GlassworkSizes.HitHeight(ButtonSize.Small));
            if (Gw.IconButton(clearGlyph, ButtonStyle.Plain, ButtonSize.Small, default, Styling.ToGlasswork(Styling.TextDim)))
            {
                text = string.Empty;
                changed = true;
            }
        }

        ImGui.PopID();
        ImGui.SetCursorScreenPos(origin);
        ImGui.Dummy(new Vector2(width, slot));
        return changed;
    }
}
