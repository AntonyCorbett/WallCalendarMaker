namespace WallCalendarMakerCore.CommonDocuments;

internal sealed class A5LandscapeDocument : CommonDocument
{
    public A5LandscapeDocument(bool drawMargin, float xMarginMillimeters, float yMarginMillimeters)
        : base(148.5F, 210F, drawMargin, xMarginMillimeters, yMarginMillimeters)
    {
    }
}