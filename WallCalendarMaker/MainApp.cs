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
                opts.MonthDefinition.Year = 2023;
                opts.MonthDefinition.Month = 10;
                opts.MonthDefinition.FirstDayOfWeek = DayOfWeek.Monday;
                opts.RowMode = RowMode.SixRows;

                // fonts
                opts.FontNameDays = "Calibri";
                opts.FontPointSizeDays = 20;
                opts.FontNameNumbers = "Calibri";
                opts.FontPointSizeNumbers = 18;

                opts.DeadBoxMode = DeadBoxMode.Invisible;
                opts.LiveBoxMode = LiveBoxMode.Opacity50;
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