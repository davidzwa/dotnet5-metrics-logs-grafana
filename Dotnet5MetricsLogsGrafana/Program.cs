using App.Metrics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.Loki;
using Serilog.Sinks.Grafana.Loki;
using System;

namespace Dotnet5MetricsLogsGrafana
{
    public class Program
    {
        private static IHostEnvironment _env;
        private static IConfiguration _appConfig;
        public static void Main(string[] args)
        {
            var credentials = new NoAuthCredentials("http://loki:3100");
            Log.Logger = new LoggerConfiguration()
#if DEBUG
                .MinimumLevel.Debug()
#else
                .MinimumLevel.Information()
#endif
                .WriteTo.Console()
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .WriteTo.GrafanaLoki("http://loki:3100")
                // .WriteTo.LokiHttp(credentials)
                .CreateLogger();
            
            Log.Information("The god of the day is {@God}", "asd");
            Log.Warning("The god of the day is {@God}", "asd");
            Log.Error("The god of the day is {@God}", "asd");

            var position = new { Latitude = 25, Longitude = 134 };
            var elapsedMs = 34;
            Log.Information("Message processed {@Position} in {Elapsed:000} ms.", position, elapsedMs);
            
            try
            {
                Log.Information("Starting up");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application start-up failed");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostContext, config) =>
                {
                    _env = hostContext.HostingEnvironment;
                    //Log.Information($"=== Running Backend in {_env.EnvironmentName} Environment setting. ===");
                    //_env.LogConfigurationAndEnvironment();

                    _appConfig = config
                        .SetBasePath(_env.ContentRootPath)
                        .AddJsonFile("appsettings.json", optional: false)
                        .AddJsonFile($"appsettings.{_env.EnvironmentName}.json", optional: false)
                        .AddEnvironmentVariables()
                        .AddCommandLine(args).Build();
                    Log.Information($"=== Found and loaded 'appsettings.{_env.EnvironmentName}.json'");
                })
                .UseSerilog()
                .ConfigureMetricsWithDefaults(builder =>
                {
                    // builder.Report.ToConsole(TimeSpan.FromSeconds(2));
                    var database = "appmetricsdemo";
                    builder.Report.ToInfluxDb("http://influxdb:8086", database, TimeSpan.FromSeconds(5));
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseStartup<Startup>();
                });
    }
}
