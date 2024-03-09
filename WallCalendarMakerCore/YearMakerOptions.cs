namespace WallCalendarMakerCore;

public class YearMakerOptions
{
    public PageSize PageSize { get; set; } = PageSize.A4;

    public int LMarginMillimeters { get; set; } = 12;

    public int TMarginMillimeters { get; set; } = 12;

    public int RMarginMillimeters { get; set; } = 12;

    public int BMarginMillimeters { get; set; } = 12;

    public MonthDefinition YearDefinition { get; set; } = new();

    public CalendarFont DayNamesFont { get; set; } = new()
    {
        Name = "Arial",
        PointSize = 14.0F,
    };

    public CalendarFont NumbersFont { get; set; } = new()
    {
        Name = "Arial",
        PointSize = 12.0F,
    };

    public bool DrawMargin { get; set; }

    public bool DrawYear { get; set; } = true;

    public void Validate()
    {
#pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one 

        if (YearDefinition.Year < DateTime.MinValue.Year)
        {
            throw new ArgumentOutOfRangeException(nameof(MonthDefinition.Year), "Year out of range!");
        }

        if (LMarginMillimeters < 0 || LMarginMillimeters > 50)
        {
            throw new ArgumentOutOfRangeException(nameof(LMarginMillimeters), "LMarginMillimeters must be between 0 and 50!");
        }

        if (RMarginMillimeters < 0 || RMarginMillimeters > 50)
        {
            throw new ArgumentOutOfRangeException(nameof(RMarginMillimeters), "RMarginMillimeters must be between 0 and 50!");
        }

        if (TMarginMillimeters < 0 || TMarginMillimeters > 50)
        {
            throw new ArgumentOutOfRangeException(nameof(TMarginMillimeters), "TMarginMillimeters must be between 0 and 50!");
        }

        if (BMarginMillimeters < 0 || BMarginMillimeters > 50)
        {
            throw new ArgumentOutOfRangeException(nameof(BMarginMillimeters), "BMarginMillimeters must be between 0 and 50!");
        }

#pragma warning restore S3928 // Parameter names used into ArgumentException constructors should match an existing one 
    }
}