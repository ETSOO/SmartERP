using com.etsoo.CoreFramework.Application;
using CoreApp.Server.Database;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

var services = builder.Services;

// Logging with OpenTelemetry
// Tracing and metrics may be added
var otlpExportOptions = configuration.GetSection("OtlpExportOptions").Get<OtlpExporterConfigs>();
if (otlpExportOptions == null)
{
    throw new NullReferenceException(nameof(otlpExportOptions));
}

builder.Logging.ClearProviders();
services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService(builder.Environment.ApplicationName))
    .WithLogging(logging => logging
        .AddConsoleExporter()
        .AddOtlpExporter(options =>
        {
            options.Protocol = otlpExportOptions.Protocol;
            options.Endpoint = otlpExportOptions.Endpoint;
            options.Headers = otlpExportOptions.Headers;
        }));

// Entity framework
var connectonString = configuration.GetConnectionString("SmartERP");
if (string.IsNullOrEmpty(connectonString))
{
    throw new Exception("SmartERP connection string not found");
}

// No need to use AddDbContextPool currently
services.AddDbContext<MyDbContext>((provider, options) =>
{
    options.UseNpgsql(connectonString)
        .UseSnakeCaseNamingConvention(); // Use snake case naming convention

    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});