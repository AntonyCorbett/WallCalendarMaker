using Svg;

namespace WallCalendarMakerCore;

internal interface IYearMaker
{
    YearMakerOptions Options { get; }

    SvgDocument Generate();

    void Generate(string path, bool useBom = true);
}