using System;
using System.Drawing;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Threading.Tasks;
using Serilog;
using WallCalendarMaker.EventArguments;
using WallCalendarMaker.Holidays;
using WallCalendarMakerCore;

namespace WallCalendarMaker;

internal sealed class MainApp
{
    private readonly ILogger _logger;

    public event EventHandler<ProgressEventArgs>? Progress;

    public MainApp(ILogger logger)
    {
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.Information("Starting");

        OnProgress("Starting...", false);

        try
        {
            var maker = new Maker(opts =>
            {
                // modify options here...
                opts.DayNamesFont = new CalendarFont
                {
                    Name = "Playfair Display",
                    Color = Color.FromArgb(71, 41, 6),
                    PointSize = 17
                };

                opts.NumbersFont = new CalendarFont
                {
                    Name = "Playfair Display",
                    Color = Color.FromArgb(97, 126, 55),
                    PointSize = 14
                };

                opts.AbbreviateDayNames = true;
                opts.BoxCornerMode = BoxCornerMode.Rounded2;

                opts.MonthDefinition.FirstDayOfWeek = DayOfWeek.Monday;
                opts.DrawMonth = false;
                opts.DrawYear = false;

                opts.LMarginMillimeters = 35;
                opts.TMarginMillimeters = 12;
                opts.RMarginMillimeters = 10;
                opts.BMarginMillimeters = 10;
            });


            for (int month = 1; month <= 12; ++month)
            {
                maker.Options.MonthDefinition.Month = month;
                maker.Options.MonthDefinition.Year = 2025;
                await AddHolidaysAsync(maker.Options, cancellationToken);
                maker.Generate($"calendar {month:D2}.svg");
            }

            var yearMaker = new YearMaker(opts =>
            {
                // modify options here...

                opts.YearDefinition.Year = 2025;
                opts.LMarginMillimeters = 30;
                opts.TMarginMillimeters = 10;
                opts.RMarginMillimeters = 10;
                opts.BMarginMillimeters = 10;

                opts.DayNamesFont = new CalendarFont
                {
                    Name = "Playfair Display",
                    Color = Color.FromArgb(71, 41, 6),
                    Bold = true,
                    PointSize = 11
                };

                opts.NumbersFont = new CalendarFont
                {
                    Name = "Source Code Pro",
                    Color = Color.FromArgb(71, 41, 6),
                    PointSize = 9
                };
            });

            yearMaker.Generate($"year {yearMaker.Options.YearDefinition.Year}.svg");
            OnProgress("Completed", false);
        }
        catch (Exception ex)
        {
            if (ex is not OperationCanceledException)
            {
                _logger.Error(ex, "Executing");
                throw;
            }
        }
    }
    private void OnProgress(ProgressEventArgs e)
    {
        Progress?.Invoke(this, e);
    }
    private void OnProgress(string message, bool verbose)
    {
        OnProgress(new ProgressEventArgs(message, verbose));
    }
    private static async Task AddHolidaysAsync(MakerOptions makerOptions, CancellationToken cancellationToken)
    {
        makerOptions.Occasions.Clear();

        foreach (var @event in
        (await GetHolidaysInMonthAsync(makerOptions, cancellationToken))
        .Where(ev => !string.IsNullOrWhiteSpace(ev.Title)))
        {
            makerOptions.Occasions.Add(new Occasion
            {
                Date = @event.Date,
                Title = @event.Title!,
                Font = new CalendarFont
                {
                    Name = "Barlow Condensed",
                    PointSize = 8.0F,
                    Color = Color.DimGray
                }
            });
        }
    }
    private static async Task<IEnumerable<HolidaysService.AnEvent>> GetHolidaysInMonthAsync(
    MakerOptions options, CancellationToken cancellationToken)
    {
        var holidays = await HolidaysService.ExecuteAsync(cancellationToken);
        if (holidays?.EnglandAndWales?.Events == null)
        {
            return Enumerable.Empty<HolidaysService.AnEvent>();
        }
        return holidays.EnglandAndWales.Events.Where(x =>
            x.Date.Year == options.MonthDefinition.Year && x.Date.Month == options.MonthDefinition.Month);
    }
}