using Microsoft.VisualBasic;
using Svg;
using WallCalendarMakerCore.CommonDocuments;

namespace WallCalendarMakerCore;

public class MakerBase
{
    protected static float PointSizeToMillimeters(float pointSize)
    {
        return (float)((pointSize * 25.4) / 72.0);
    }

    protected static float UnitSizeToMillimeters(float unitSize)
    {
        return (float)(unitSize * 25.4) / 96.0F;
    }

    protected SvgDocument CreateBlankDocument(
        PageSize pageSize, bool drawMargin, int lMarginMillimeters, int tMarginMillimeters, int rMarginMillimeters, int bMarginMillimeters)
    {
        return pageSize switch
        {
            PageSize.A3 => new A3LandscapeDocument(
                drawMargin, lMarginMillimeters, tMarginMillimeters, rMarginMillimeters, bMarginMillimeters),
            PageSize.A4 => new A4LandscapeDocument(
                drawMargin, lMarginMillimeters, tMarginMillimeters, rMarginMillimeters, bMarginMillimeters),
            PageSize.A5 => new A5LandscapeDocument(
                drawMargin, lMarginMillimeters, tMarginMillimeters, rMarginMillimeters, bMarginMillimeters),
            _ => throw new NotSupportedException()
        };
    }

    protected SvgText GenerateText(string text, CalendarFont font, string? id = null)
    {
        return new SvgText(text)
        {
            ID = id,
            Font = font.Name,
            FontSize = new SvgUnit(SvgUnitType.Point, font.PointSize),
            FontStyle = font.Italic ? SvgFontStyle.Italic : SvgFontStyle.Normal,
            FontWeight = font.Bold ? SvgFontWeight.Bold : SvgFontWeight.Normal,
            Fill = new SvgColourServer(font.Color),
            Stroke = SvgPaintServer.None,
            StrokeWidth = 0,
        };
    }
}