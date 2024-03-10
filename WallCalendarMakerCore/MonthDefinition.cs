namespace WallCalendarMakerCore;

public class MonthDefinition
{
    public int Year { get; set; } = DateTime.Today.Year;

    public int Month { get; set; } = DateTime.Today.Month;

    public DayOfWeek FirstDayOfWeek { get; set; } = DayOfWeek.Monday;

    public void Validate()
    {
#pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one 

        if (Year < 1900 || Year > 3000)
        {
            throw new ArgumentOutOfRangeException(nameof(Year), "Year must be between 1900 and 3000!");
        }

        if (Month < 1 || Month > 12)
        {
            throw new ArgumentOutOfRangeException(nameof(Month), "Month must be between 1 and 12!");
        }

#pragma warning restore S3928 // Parameter names used into ArgumentException constructors should match an existing one 
    }
}