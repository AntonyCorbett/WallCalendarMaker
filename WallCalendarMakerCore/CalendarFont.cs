using System.Drawing;

namespace WallCalendarMakerCore;

public class CalendarFont
{
    public string Name { get; set; } = string.Empty;

    public float PointSize { get; set; }

    public CalendarFontWeight Weight { get; set; }

    public bool Italic { get; set; }

    public Color Color { get; set; } = Color.Black;
}