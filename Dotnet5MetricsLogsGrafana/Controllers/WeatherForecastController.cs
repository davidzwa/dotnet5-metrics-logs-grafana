using App.Metrics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Dotnet5MetricsLogsGrafana.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly IMetrics _metrics;
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(
            IMetrics metrics,
            ILogger<WeatherForecastController> logger)
        {
            _metrics = metrics;
            _logger = logger;
        }

        [HttpGet("error")]
        public void GetError()
        {
            int milliseconds = 5000;
            Thread.Sleep(milliseconds);
            throw new Exception("asd");
        }

        [HttpGet("fast")]
        public void GetFast()
        {
            int milliseconds = 50;
            _logger.LogInformation("asd");
            _logger.LogWarning("asdwarn");
            Thread.Sleep(milliseconds);
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            _metrics.Measure.Counter.Increment(MetricsRegistry.SampleCounter);
            int milliseconds = 5000;
            Thread.Sleep(milliseconds);
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}
