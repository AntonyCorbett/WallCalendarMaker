using Svg;

namespace WallCalendarMakerCore.CommonDocuments;

internal class A4LandscapeDocument : CommonDocument
{
    public A4LandscapeDocument()
    {
        Height = new SvgUnit(SvgUnitType.Millimeter, 210);
        Width = new SvgUnit(SvgUnitType.Millimeter, 297);
    }
}