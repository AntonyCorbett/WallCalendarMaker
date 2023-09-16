namespace WallCalendarMakerCore;

public class MakerOptions
{
    public PageSize PageSize { get; set; } = PageSize.A4;

    public int XMarginMillimeters { get; set; } = 12;

    public int YMarginMillimeters { get; set; } = 12;

    public MonthDefinition MonthDefinition { get; set; } = new();

    public string FontNameDays { get; set; } = "Arial";

    public float FontPointSizeDays { get; set; } = 18.0F;

    public string FontNameNumbers { get; set; } = "Arial";

    public float FontPointSizeNumbers { get; set; } = 14.0F;

    public RowMode RowMode { get; set; } = RowMode.SixRows;

    public DeadBoxMode DeadBoxMode { get; set; } = DeadBoxMode.Invisible;

    public LiveBoxMode LiveBoxMode { get; set; } = LiveBoxMode.Visible;

    public bool DrawMargin { get; set; }

    public BoxCornerMode BoxCornerMode { get; set; } = BoxCornerMode.Rounded;

    public void Validate()
    {
#pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one 

        if (XMarginMillimeters < 0 || XMarginMillimeters > 50)
        {
            throw new ArgumentOutOfRangeException(nameof(XMarginMillimeters), "XMarginMillimeters must be between 0 and 50!");
        }

        if (YMarginMillimeters < 0 || YMarginMillimeters > 50)
        {
            throw new ArgumentOutOfRangeException(nameof(XMarginMillimeters), "YMarginMillimeters must be between 0 and 50!");
        }

#pragma warning restore S3928 // Parameter names used into ArgumentException constructors should match an existing one 
    }
}