using Serilog;
using WallCalendarMaker.EventArguments;
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
                opts.XMarginMillimeters = 20;
                opts.YMarginMillimeters = 10;
                opts.MonthDefinition.Year = 2023;
                opts.MonthDefinition.Month = 4;
                opts.MonthDefinition.FirstDayOfWeek = DayOfWeek.Monday;
                opts.RowMode = RowMode.SixRows;
                opts.BoxCornerMode = BoxCornerMode.Rounded2;
                //opts.LiveBoxMode = LiveBoxMode.Opacity25;

                // fonts
                opts.DayNamesFont = new CalendarFont
                {
                    Name = "Constantia",
                    PointSize = 14,
                    Bold = true,
                };

                opts.NumbersFont = new CalendarFont
                {
                    Name = "Constantia",
                    PointSize = 16,
                };

                opts.MonthFont = new CalendarFont
                {
                    Name = "Constantia",
                    PointSize = 36,
                    Bold = true,
                };

                opts.YearFont = new CalendarFont
                {
                    Name = "Constantia",
                    PointSize = 36,
                    Bold = true,
                };

                opts.DrawMargin = false;
                opts.DrawOutlineBox = false;

                opts.DrawMonth = false;
                opts.DrawYear = false;
            });

            await maker.GenerateAsync("myfile.svg");

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
}