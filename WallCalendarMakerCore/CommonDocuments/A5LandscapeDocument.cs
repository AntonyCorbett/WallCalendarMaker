using Svg;

namespace WallCalendarMakerCore.CommonDocuments;

internal class A5LandscapeDocument : CommonDocument
{
    public A5LandscapeDocument()
    {
        Height = new SvgUnit(SvgUnitType.Millimeter, 148.5F);
        Width = new SvgUnit(SvgUnitType.Millimeter, 210);
    }
}