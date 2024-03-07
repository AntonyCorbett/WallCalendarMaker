using Svg;

namespace WallCalendarMakerCore.CommonDocuments;

internal class CommonDocument : SvgDocument
{
    public CommonDocument(
        float heightMillimeters,
        float widthMillimeters,
        bool drawMargin,
        float lMarginMillimeters,
        float tMarginMillimeters,
        float rMarginMillimeters,
        float bMarginMillimeters)
    {
        Ppi = 300;

        Height = new SvgUnit(SvgUnitType.Millimeter, heightMillimeters);
        Width = new SvgUnit(SvgUnitType.Millimeter, widthMillimeters);

        if (drawMargin)
        {
            DrawMargin(lMarginMillimeters, tMarginMillimeters, rMarginMillimeters, bMarginMillimeters, heightMillimeters, widthMillimeters);
        }
    }

    protected void DrawMargin(float lMarginMillimeters, float tMarginMillimeters, float rMarginMillimeters, float bMarginMillimeters, float pageHeightMillimeters, float pageWidthMillimeters)
    {
        Children.Add(new SvgRectangle
        {
            X = new SvgUnit(SvgUnitType.Millimeter, lMarginMillimeters),
            Y = new SvgUnit(SvgUnitType.Millimeter, tMarginMillimeters),
            Width = new SvgUnit(SvgUnitType.Millimeter, pageWidthMillimeters - lMarginMillimeters - rMarginMillimeters),
            Height = new SvgUnit(SvgUnitType.Millimeter, pageHeightMillimeters - tMarginMillimeters - bMarginMillimeters),
            Stroke = new SvgColourServer(System.Drawing.Color.DeepSkyBlue),
            StrokeWidth = new SvgUnit(SvgUnitType.Pixel, 1),
            Fill = SvgPaintServer.None,
        });
    }
}