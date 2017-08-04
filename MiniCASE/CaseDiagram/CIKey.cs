using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MiniCASE
{
    /// <summary>
    /// Used primary in CDGraphics.DrawShape
    /// </summary>
    public enum CIKey
    {
        None = 0,
        StrokeColor,
        BackgroundColor,
        FillTriangle,
        DrawTriangle,
        FillRectangle,
        DrawRectangle,
        FillEllipse,
        DrawEllipse,
        DrawText,
        DrawImage,
        SetBoundsByFitImage,
        SetBoundsByFillImage
    }

    /// <summary>
    /// Main usage for storing is CDInstruction.ReadFrom and CDInstruction.WriteTo
    /// </summary>
    public enum CIParam
    {
        RelativeRect,
        RelativePoint,
        RelativeTriangle,
        Width,
        WidthId,
        Height,
        HeightId,
        Text,
        TextId,
        Color,
        ColorId,
        FontSize,
        FontSizeId,
        TextAlign,
        TextAlignId,
        TextPadding,
        Image,
        ImageId
    }

    [Flags]
    public enum CICondition
    {
        Selected = 0x1,
        NotSelected = 0x10,
        Highlighted = 0x100,
        NotHighlighted = 0x1000,
        Allways = 0x11111111
    }

}
