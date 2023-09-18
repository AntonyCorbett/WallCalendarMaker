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
                opts.XMarginMillimeters = 40;
                opts.MonthDefinition.Year = 2023;
                opts.MonthDefinition.Month = 10;
                opts.MonthDefinition.FirstDayOfWeek = DayOfWeek.Monday;
                opts.RowMode = RowMode.SixRows;

                // fonts
                //opts.DayNamesFont = new CalendarFont
                //{
                //    Name = "Bradley Hand ITC",
                //    PointSize = 20,
                //    Bold = true,
                //};

                //opts.NumbersFont = new CalendarFont
                //{
                //    Name = "Calibri",
                //    PointSize = 16,
                //};

                //opts.MonthFont = new CalendarFont
                //{
                //    Name = "Calibri",
                //    PointSize = 28,
                //};

                //opts.YearFont = new CalendarFont
                //{
                //    Name = "Bradley Hand ITC",
                //    PointSize = 28,
                //    Bold = true,
                //    Italic = true
                //};

                //opts.DrawMargin = true;
                //opts.DrawOutlineBox = true;

                opts.DrawMonth = true;
                opts.DrawYear = true;
            });

            maker.Generate("myfile.svg");

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