using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Glasswork;
using Glasswork.Backend;
using System.Numerics;

namespace AutoSightseeingLog.Windows.Components;

internal static class MetricTile
{
    public const float DefaultHeight = 66f;

    private const float PadX = 13f;
    private const float PadY = 10f;
    private const float DotRadius = 3f;
    private const float DotGap = 6f;
    private const float DetailGap = 8f;
    private const float AccentTint = 0.05f;

    public static void Draw(string label, string value, string? detail, Vector4 accent, float width, float height = DefaultHeight)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var look = Frame.Look;
        var size = new Vector2(width, height * scale);
        var origin = ImGui.GetCursorScreenPos();
        var end = origin + size;
        var drawList = ImGui.GetWindowDrawList();

        Paint.Card(drawList, origin, end, Styling.CardRounding * scale, accent, AccentTint);

        var padX = PadX * scale;
        var padY = PadY * scale;
        var labelHeight = TextPaint.LineHeight(TextRole.Footnote, look, scale);
        var dotRadius = DotRadius * scale;
        drawList.AddCircleFilled(new Vector2(origin.X + padX + dotRadius, origin.Y + padY + labelHeight * 0.5f), dotRadius, Paint.Col(accent));
        var labelLeft = origin.X + padX + dotRadius * 2f + DotGap * scale;
        var labelBox = new Rect(new Vector2(labelLeft, origin.Y + padY), new Vector2(end.X - padX, origin.Y + padY + labelHeight));
        TextPaint.DrawInBox(drawList, labelBox, TextDraw.Upper(label), TextRole.Footnote, look.SecondaryLabel, look, scale);

        var valueRight = end.X - padX;
        var valueHeight = TextPaint.LineHeight(TextRole.Headline, look, scale);
        if (!string.IsNullOrEmpty(detail))
        {
            var detailSize = TextPaint.Measure(detail, TextRole.Footnote, look, scale);
            var detailTop = end.Y - padY - detailSize.Y;
            TextPaint.Draw(drawList, new Vector2(end.X - padX - detailSize.X, detailTop), detail, TextRole.Footnote, look.SecondaryLabel, look, scale);
            valueRight -= detailSize.X + DetailGap * scale;
        }

        var valueBox = new Rect(new Vector2(origin.X + padX, end.Y - padY - valueHeight), new Vector2(valueRight, end.Y - padY));
        TextPaint.DrawInBox(drawList, valueBox, value, TextRole.Headline, look.Label, look, scale);

        ImGui.Dummy(size);
    }
}
