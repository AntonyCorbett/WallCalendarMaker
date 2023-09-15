namespace WallCalendarMakerCore;

public class MakerOptions
{
    public PageSize PageSize { get; set; } = PageSize.A4;

    public int XMarginMillimeters { get; set; } = 15;

    public int YMarginMillimeters { get; set; } = 15;

    public MonthDefinition MonthDefinition { get; set; } = new();

    public string FontName { get; set; } = "Arial";

    public float FontPointSize { get; set; } = 10.0F;

    public RowMode RowMode { get; set; } = RowMode.FiveRows;

    public DeadBoxMode DeadBoxMode { get; set; } = DeadBoxMode.Opacity25;

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