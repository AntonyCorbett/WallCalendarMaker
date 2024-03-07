namespace WallCalendarMakerCore.CommonDocuments;

internal sealed class A5LandscapeDocument : CommonDocument
{
    public A5LandscapeDocument(bool drawMargin, float lMarginMillimeters, float tMarginMillimeters, float rMarginMillimeters, float bMarginMillimeters)
        : base(148.5F, 210F, drawMargin, lMarginMillimeters, tMarginMillimeters, rMarginMillimeters, bMarginMillimeters)
    {
    }
}