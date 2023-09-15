using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;
using System.Diagnostics;

namespace WallCalendarMaker
{
    internal static class Program
    {
        private const string AppName = "WallCalendarMaker";
        private static CancellationTokenSource? _cancellationTokenSource;
        private static bool _verboseOutput;

        private static async Task Main(string[] args)
        {
            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                Console.CancelKeyPress += HandleCancel;

                DisplayWelcome();

                var host = CreateHostBuilder(args).Build();
                var app = host.Services.GetRequiredService<MainApp>();
                var options = host.Services.GetRequiredService<Options>();

                _verboseOutput = options.Verbose;

                if (!options.IsValid())
                {
                    DisplayUsage();
                    return;
                }

                app.Progress += ProgressEventHandler;

                var stopwatch = Stopwatch.StartNew();
                await app.ExecuteAsync(_cancellationTokenSource.Token);
                var duration = stopwatch.Elapsed;

                DisplayResult(duration);
            }
            catch (OperationCanceledException)
            {
                Environment.ExitCode = -1;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Environment.ExitCode = -2;
            }
        }

        private static void DisplayResult(TimeSpan duration)
        {
            Console.WriteLine();
            Console.WriteLine($"Total time: {duration}");
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var logger = ConfigureLogging();

            var builder = new ConfigurationBuilder().AddCommandLine(args);
            var config = builder.Build();

            var options = GetOptions(config);

            return Host.CreateDefaultBuilder(args)
                .ConfigureServices((_, services) =>
                {
                    services.AddTransient<MainApp>();
                    services.AddTransient<ILogger>(_ => logger);
                    services.AddTransient(_ => options);
                });
        }

        private static Options GetOptions(IConfigurationRoot config)
        {
            // var durationStr = config["maxDuration"];
            // int.TryParse(durationStr, out var duration);

            // build options here

            var verboseStr = config["verbose"];
            var verbose = verboseStr != null && verboseStr.Equals("true", StringComparison.OrdinalIgnoreCase);

            return new Options
            {
                Verbose = verbose
            };
        }

        private static void ProgressEventHandler(object? sender, EventArguments.ProgressEventArgs e)
        {
            if (!e.IsVerbose || _verboseOutput)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static Logger ConfigureLogging()
        {
            return new LoggerConfiguration()
                .WriteTo.File("logs\\log-.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 10)
                .CreateLogger();
        }

        private static void HandleCancel(object? sender, ConsoleCancelEventArgs args)
        {
            Console.WriteLine();
            Console.WriteLine("Cancelling...");
            Console.WriteLine();

            // Set the Cancel property to true to prevent the process from terminating.
            args.Cancel = true;

            // then cancel in a controlled way...
            _cancellationTokenSource?.Cancel();
        }

        private static void DisplayWelcome()
        {
            Console.WriteLine(AppName);
            Console.WriteLine(new string('=', AppName.Length));
        }

        private static void DisplayUsage()
        {
            Console.WriteLine();
            Console.WriteLine("USAGE");
            Console.WriteLine("-----");

            Console.WriteLine();
            Console.WriteLine("--verbose     set to 'true' for verbose console output");
            Console.WriteLine();
        }
    }
}