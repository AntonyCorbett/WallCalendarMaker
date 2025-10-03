using System.Drawing;
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
                    Name = "Fraunces",
                    Color = Color.FromArgb(71, 41, 6),
                    Weight = CalendarFontWeight.W100,
                    PointSize = 17
                };

                opts.NumbersFont = new CalendarFont
                {
                    Name = "Fraunces",
                    Color = Color.FromArgb(97, 126, 55),
                    Weight = CalendarFontWeight.W100,
                    PointSize = 14
                };

                opts.AbbreviateDayNames = true;
                opts.BoxCornerMode = BoxCornerMode.Rounded2;

                opts.MonthDefinition.FirstDayOfWeek = DayOfWeek.Monday;
                opts.DrawMonth = false;
                opts.DrawYear = false;

                opts.LMarginMillimeters = 35;
                opts.TMarginMillimeters = 22;
                opts.RMarginMillimeters = 10;
                opts.BMarginMillimeters = 10;
            });


            var standardOccasionFont = CreateOccasionFont(Color.DimGray);

            for (int month = 1; month <= 12; ++month)
            {
                maker.Options.Occasions.Clear();

                maker.Options.MonthDefinition.Month = month;
                maker.Options.MonthDefinition.Year = 2026;

                await AddHolidaysAsync(maker.Options, standardOccasionFont, cancellationToken);
                AddSpecialDays(maker.Options, standardOccasionFont);

                maker.Generate($"calendar {month:D2}.svg");
            }

            CreateWholeYear(maker.Options.MonthDefinition.Year);
            CreateWholeYear(maker.Options.MonthDefinition.Year + 1);

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

    private void CreateWholeYear(int year)
    {
        var yearMaker = new YearMaker(opts =>
        {
            // modify options here...

            opts.YearDefinition.Year = year;
            opts.LMarginMillimeters = 30;
            opts.TMarginMillimeters = 10;
            opts.RMarginMillimeters = 10;
            opts.BMarginMillimeters = 10;

            opts.DayNamesFont = new CalendarFont
            {
                Name = "Fraunces",
                Color = Color.FromArgb(71, 41, 6),
                Weight = CalendarFontWeight.W700,
                PointSize = 12
            };

            opts.YearDefinition.FirstDayOfWeek = DayOfWeek.Monday;

            opts.MonthHeaderBackgroundColor = Color.FromArgb(71, 41, 6);

            opts.MonthNamesFont = new CalendarFont
            {
                Name = "Fraunces",
                Color = Color.White,
                Weight = CalendarFontWeight.W600,
                PointSize = 12
            };

            opts.NumbersFont = new CalendarFont
            {
                Name = "Source Code Pro",
                Color = Color.FromArgb(97, 126, 55),
                Weight = CalendarFontWeight.Normal,
                PointSize = 9
            };
        });

        yearMaker.Generate($"year {yearMaker.Options.YearDefinition.Year}.svg");
    }

    private void OnProgress(ProgressEventArgs e)
    {
        Progress?.Invoke(this, e);
    }

    private void OnProgress(string message, bool verbose)
    {
        OnProgress(new ProgressEventArgs(message, verbose));
    }

    private static CalendarFont CreateOccasionFont(Color color)
    {
        return new CalendarFont
        {
            Name = "Inter",
            PointSize = 9.0F,
            Color = color,
        };
    }

    private static void AddSpecialDays(MakerOptions makerOptions, CalendarFont font)
    {
        var specialMiscDays = GetSpecialMiscDays(makerOptions.MonthDefinition.Year)
            .Where(x => x.Date.Year == makerOptions.MonthDefinition.Year && 
                        x.Date.Month == makerOptions.MonthDefinition.Month);

        foreach (var specialDay in specialMiscDays)
        {
            AddOccasion(makerOptions.Occasions, specialDay.Date, specialDay.Title!, font);
        }

        //var foafFont = CreateOccasionFont(Color.FromArgb(97, 126, 55));

        //var specialFoafDays = GetSpecialFoafDays(makerOptions.MonthDefinition.Year)
        //    .Where(x => x.Date.Year == makerOptions.MonthDefinition.Year &&
        //                x.Date.Month == makerOptions.MonthDefinition.Month);

        //foreach (var specialDay in specialFoafDays)
        //{
        //    AddOccasion(makerOptions.Occasions, specialDay.Date, specialDay.Title!, foafFont);
        //}
    }

    private static IEnumerable<HolidaysService.AnEvent> GetSpecialFoafDays(int year)
    {
        yield return new HolidaysService.AnEvent(new DateTime(2025, 4, 20), "FOAF Bird Walk");
        yield return new HolidaysService.AnEvent(new DateTime(2025, 5, 17), "FOAF Tree Walk");
        yield return new HolidaysService.AnEvent(new DateTime(2025, 6, 21), "FOAF Meadow Walk");
        yield return new HolidaysService.AnEvent(new DateTime(2025, 7, 16), "FOAF Bat Walk");
        yield return new HolidaysService.AnEvent(new DateTime(2025, 8, 8), "FOAF Moth Evening");

        foreach (var item in GetFirstSaturdayLitterPicks(year))
        {
            yield return item;
        }
    }

    private static IEnumerable<HolidaysService.AnEvent> GetSpecialMiscDays(int year)
    {
        yield return GetLionsGrandShow(year);
        foreach (var ev in GetRunnersEvents())
        {
            yield return ev;
        }
    }

    private static IEnumerable<HolidaysService.AnEvent> GetRunnersEvents()
    {
        // 4th May – Mayday Mile in Abbey Fields
        // 8th June – Two Castles Run 2025, Warwick to Kenilworth Castle
        // 14th September – Kenilworth Half Marathon, inc Kenilworth Castle
        
        yield return new HolidaysService.AnEvent(new DateTime(2025, 5, 4), "Mayday Mile");
        yield return new HolidaysService.AnEvent(new DateTime(2025, 6, 8), "Two Castles Run");
        yield return new HolidaysService.AnEvent(new DateTime(2026, 9, 6), "Half Marathon");
    }

    private static HolidaysService.AnEvent GetLionsGrandShow(int year)
    {
        // always the 2nd Saturday in June
        return new HolidaysService.AnEvent(GetFirstSaturdayOfMonth(year, 6).AddDays(7), "Lions Grand Show");
    }

    private static IEnumerable<HolidaysService.AnEvent> GetFirstSaturdayLitterPicks(int year)
    {
        // first Saturday between April and October inclusive

        for (var month = 4; month <= 10; ++month)
        {
            yield return new HolidaysService.AnEvent(GetFirstSaturdayOfMonth(year, month), "FOAF Litter Pick");
        }
    }

    private static DateTime GetFirstSaturdayOfMonth(int year, int month)
    {
        var dt = new DateTime(year, month, 1);
        while (dt.DayOfWeek != DayOfWeek.Saturday)
        {
            dt = dt.AddDays(1);
        }

        return dt;
    }

    private static async Task AddHolidaysAsync(
        MakerOptions makerOptions, CalendarFont standardFont, CancellationToken cancellationToken)
    {
        foreach (var @event in (await GetHolidaysInMonthAsync(makerOptions, cancellationToken))
            .Where(ev => !string.IsNullOrWhiteSpace(ev.Title)))
        {
            AddOccasion(makerOptions.Occasions, @event.Date, @event.Title!, standardFont);
        }
    }

    private static void AddOccasion(List<Occasion> occasions, DateTime theDate, string text, CalendarFont font)
    {
        occasions.Add(new Occasion
        {
            Date = theDate,
            Title = text,
            Font = font
        });
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