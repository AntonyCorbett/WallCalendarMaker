using System.Drawing;

namespace WallCalendarMakerCore;

public class Occasion
{
    public string Title { get; set; } = string.Empty;

    public DateTime Date { get; set; }

    public CalendarFont Font { get; set; } = new()
    {
        Name = "Arial",
        PointSize = 7.0F,
        Color = Color.DarkGray
    };
}