namespace WallCalendarMakerCore.CommonDocuments;

internal sealed class A4LandscapeDocument : CommonDocument
{
    public A4LandscapeDocument(bool drawMargin, float xMarginMillimeters, float yMarginMillimeters)
        : base(210F, 297F, drawMargin, xMarginMillimeters, yMarginMillimeters)
    {
    }
}