namespace WallCalendarMakerCore;

public class MakerOptions
{
    public PageSize PageSize { get; set; } = PageSize.A4;

    public int LMarginMillimeters { get; set; } = 12;

    public int TMarginMillimeters { get; set; } = 12;

    public int RMarginMillimeters { get; set; } = 12;

    public int BMarginMillimeters { get; set; } = 12;

    public MonthDefinition MonthDefinition { get; set; } = new();

    public bool AbbreviateDayNames { get; set; }

    public CalendarFont MonthFont { get; set; } = new()
    {
        Name = "Arial",
        PointSize = 22.0F,
    };

    public CalendarFont YearFont { get; set; } = new()
    {
        Name = "Arial",
        PointSize = 22.0F,
    };

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

    public RowMode RowMode { get; set; } = RowMode.SixRows;

    public DeadBoxMode DeadBoxMode { get; set; } = DeadBoxMode.Invisible;

    public LiveBoxMode LiveBoxMode { get; set; } = LiveBoxMode.Visible;

    public bool DrawMargin { get; set; }

    public BoxCornerMode BoxCornerMode { get; set; } = BoxCornerMode.Rounded2;

    public bool IsBoxRounded => BoxCornerMode != BoxCornerMode.Normal && BoxCornerMode != BoxCornerMode.Merge;

    public bool DrawMonth { get; set; } = true;

    public bool DrawYear { get; set; } = true;

    public bool DrawOutlineBox { get; set; }

    public List<Occasion> Occasions { get; set; } = new();

    public void Validate()
    {
#pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one 

        if (MonthDefinition.Year < DateTime.MinValue.Year)
        {
            throw new ArgumentOutOfRangeException(nameof(MonthDefinition.Year), "Year out of range!");
        }

        if (MonthDefinition.Month < 1 || MonthDefinition.Month > 12)
        {
            throw new ArgumentOutOfRangeException(nameof(MonthDefinition.Month), "Month must be between 1 and 12!");
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