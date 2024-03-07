namespace WallCalendarMakerCore.CommonDocuments;

internal sealed class A3LandscapeDocument : CommonDocument
{
    public A3LandscapeDocument(bool drawMargin, float lMarginMillimeters, float tMarginMillimeters, float rMarginMillimeters, float bMarginMillimeters)
        : base(297F, 420F, drawMargin, lMarginMillimeters, tMarginMillimeters, rMarginMillimeters, bMarginMillimeters)
    {
    }
}