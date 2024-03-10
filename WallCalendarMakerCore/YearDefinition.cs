namespace WallCalendarMakerCore;

public class YearDefinition
{
    public int Year { get; set; } = DateTime.Today.Year;

    public DayOfWeek FirstDayOfWeek { get; set; } = DayOfWeek.Monday;

    public void Validate()
    {
#pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one 

        if (Year < 1900 || Year > 3000)
        {
            throw new ArgumentOutOfRangeException(nameof(Year), "Year must be between 1900 and 3000!");
        }

#pragma warning restore S3928 // Parameter names used into ArgumentException constructors should match an existing one 
    }
}