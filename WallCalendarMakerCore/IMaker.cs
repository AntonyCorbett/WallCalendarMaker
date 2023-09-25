using Svg;

namespace WallCalendarMakerCore;

internal interface IMaker
{
    MakerOptions Options { get; }

    SvgDocument Generate();

    void Generate(string path, bool useBom = true);
}