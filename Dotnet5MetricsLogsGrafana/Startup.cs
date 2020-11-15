using App.Metrics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Serilog;
using System;
using System.Collections.Generic;

namespace Dotnet5MetricsLogsGrafana
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddMetrics(options =>
                {
                    options.Configuration.Configure(config =>
                    {
                        config.AddAppTag("Dotnet5MetricsLogsGrafana");
                        config.Enabled = true;
                        config.ReportingEnabled = true;
                    });
                    var database = "appmetricsdemo";
                    options.Report.ToInfluxDb("http://influxdb:8086", database, TimeSpan.FromSeconds(5));
                })
                .AddMetricsReportingHostedService()
                .AddMetricsTrackingMiddleware()
                .AddAppMetricsHealthPublishing()
                .AddMetricsEndpoints()
                .AddHealthChecks();

            services.AddMetricsTrackingMiddleware(options =>
            {
                options.IgnoredHttpStatusCodes = new List<int> { 404 };
            });

            services.AddControllers().AddMetrics();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Dotnet5MetricsLogsGrafana", Version = "v1" });
            });
            services.AddApplicationInsightsTelemetry();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            IWebHostEnvironment env
        )
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Dotnet5MetricsLogsGrafana v1"));
            }
            app.UseSerilogRequestLogging();
            app.UseMetricsAllMiddleware();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            app.UseMetricsAllEndpoints();
        }
    }
}
