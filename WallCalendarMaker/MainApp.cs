﻿using System.Drawing;
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
            });

            await AddHolidaysAsync(maker.Options, cancellationToken);

            maker.Generate("calendar.svg");

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
                    Name = "Arial",
                    PointSize = 7.0F,
                    Color = Color.DarkGray
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