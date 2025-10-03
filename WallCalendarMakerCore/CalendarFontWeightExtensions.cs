using System;
using Svg;

namespace WallCalendarMakerCore;

public static class CalendarFontWeightExtensions
{
    public static SvgFontWeight ToSvgFontWeight(this CalendarFontWeight w)
    {
        return w switch
        {
            // Use numeric forms for Normal/Bold to emit 400/700 rather than keywords
            CalendarFontWeight.Normal => SvgFontWeight.W400,
            CalendarFontWeight.Bold => SvgFontWeight.W700,
            CalendarFontWeight.W900 => SvgFontWeight.W900,
            CalendarFontWeight.W800 => SvgFontWeight.W800,
            CalendarFontWeight.W700 => SvgFontWeight.W700,
            CalendarFontWeight.W600 => SvgFontWeight.W600,
            CalendarFontWeight.W500 => SvgFontWeight.W500,
            CalendarFontWeight.W400 => SvgFontWeight.W400,
            CalendarFontWeight.W300 => SvgFontWeight.W300,
            CalendarFontWeight.W200 => SvgFontWeight.W200,
            CalendarFontWeight.W100 => SvgFontWeight.W100,
            _ => throw new ArgumentOutOfRangeException(nameof(w))
        };
    }

    public static string ToCssFontWeightString(this CalendarFontWeight w) => w switch
    {
        CalendarFontWeight.Normal => "400",
        CalendarFontWeight.Bold => "700",
        CalendarFontWeight.W900 => "900",
        CalendarFontWeight.W800 => "800",
        CalendarFontWeight.W700 => "700",
        CalendarFontWeight.W600 => "600",
        CalendarFontWeight.W500 => "500",
        CalendarFontWeight.W400 => "400",
        CalendarFontWeight.W300 => "300",
        CalendarFontWeight.W200 => "200",
        CalendarFontWeight.W100 => "100",
        _ => throw new ArgumentOutOfRangeException(nameof(w))
    };
}