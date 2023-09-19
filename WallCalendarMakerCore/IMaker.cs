using Svg;

namespace WallCalendarMakerCore;

internal interface IMaker
{
    MakerOptions Options { get; }

    Task<SvgDocument> GenerateAsync();

    Task GenerateAsync(string path, bool useBom = true);
}