using Svg;

namespace WallCalendarMakerCore.CommonDocuments;

internal class A3LandscapeDocument : CommonDocument
{
    public A3LandscapeDocument()
    {
        Height = new SvgUnit(SvgUnitType.Millimeter, 297);
        Width = new SvgUnit(SvgUnitType.Millimeter, 420);
    }
}