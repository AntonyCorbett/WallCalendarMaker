using Svg;

namespace WallCalendarMakerCore.CommonDocuments;

internal class CommonDocument : SvgDocument
{
    public CommonDocument(
        float heightMillimeters,
        float widthMillimeters,
        bool drawMargin,
        float xMarginMillimeters,
        float yMarginMillimeters)
    {
        Ppi = 300;

        Height = new SvgUnit(SvgUnitType.Millimeter, heightMillimeters);
        Width = new SvgUnit(SvgUnitType.Millimeter, widthMillimeters);

        if (drawMargin)
        {
            DrawMargin(xMarginMillimeters, yMarginMillimeters, heightMillimeters, widthMillimeters);
        }
    }

    protected void DrawMargin(float xMarginMillimeters, float yMarginMillimeters, float pageHeightMillimeters, float pageWidthMillimeters)
    {
        Children.Add(new SvgRectangle
        {
            X = new SvgUnit(SvgUnitType.Millimeter, xMarginMillimeters),
            Y = new SvgUnit(SvgUnitType.Millimeter, yMarginMillimeters),
            Width = new SvgUnit(SvgUnitType.Millimeter, pageWidthMillimeters - (2 * xMarginMillimeters)),
            Height = new SvgUnit(SvgUnitType.Millimeter, pageHeightMillimeters - (2 * yMarginMillimeters)),
            Stroke = new SvgColourServer(System.Drawing.Color.DeepSkyBlue),
            StrokeWidth = new SvgUnit(SvgUnitType.Pixel, 1),
            Fill = SvgPaintServer.None,
        });
    }
}