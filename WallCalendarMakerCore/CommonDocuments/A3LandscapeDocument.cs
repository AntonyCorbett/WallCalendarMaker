namespace WallCalendarMakerCore.CommonDocuments;

internal sealed class A3LandscapeDocument : CommonDocument
{
    public A3LandscapeDocument(bool drawMargin, float xMarginMillimeters, float yMarginMillimeters)
        : base(297F, 420F, drawMargin, xMarginMillimeters, yMarginMillimeters)
    {
    }
}