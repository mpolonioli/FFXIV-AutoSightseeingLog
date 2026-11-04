using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Glasswork;
using System.Numerics;

namespace AutoSightseeingLog.Windows.Shell;

internal static class Dock
{
    public static void Background(ImDrawListPtr dl, Vector2 origin, Vector2 end, float windowRounding)
    {
        Bar(dl, origin, end, new Corners(0f, 0f, windowRounding, windowRounding));
        Paint.Hairline(dl, origin, new Vector2(end.X, origin.Y));
    }

    // The window's bars wear the theme's bar material: real glass on the glass themes, the theme's own fill on the rest.
    public static void Bar(ImDrawListPtr dl, Vector2 min, Vector2 max, Corners corners)
    {
        var host = Plugin.Instance.GlassworkHost;
        var look = host.Look;
        Glasswork.Backend.Paint.Material(dl, new Rect(min, max), corners, look.CornerStyle, in look.BarMaterial, look, host.Glass, ImGuiHelpers.GlobalScale);
    }
}
