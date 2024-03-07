namespace WallCalendarMakerCore.CommonDocuments;

internal sealed class A4LandscapeDocument : CommonDocument
{
    public A4LandscapeDocument(bool drawMargin, float lMarginMillimeters, float tMarginMillimeters, float rMarginMillimeters, float bMarginMillimeters)
        : base(210F, 297F, drawMargin, lMarginMillimeters, tMarginMillimeters, rMarginMillimeters, bMarginMillimeters)
    {
    }
}