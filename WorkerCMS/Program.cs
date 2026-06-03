using com.etsoo.CoreFramework.Application;
using com.etsoo.MessageQueue.LocalRabbitMQ;
using com.etsoo.MessageQueue.QueueProcessors;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using PlatformShared.Database;
using WorkerCMS.Processors.Person;

var builder = Host.CreateApplicationBuilder(args);
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

var logConnectionString = configuration.GetConnectionString("SmartERPLog");
if (string.IsNullOrEmpty(logConnectionString))
{
    throw new Exception("SmartERPLog connection string not found");
}

// services.AddDbContextPool
services.AddDbContext<MyDbContext>((provider, options) =>
{
    options.UseNpgsql(connectonString);

    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
}, ServiceLifetime.Singleton);

services.AddDbContext<LogDbContext>((provider, options) =>
{
    options.UseNpgsql(logConnectionString);

    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
}, ServiceLifetime.Singleton);

var consumerOptions = configuration.GetSection("RabbitMQConsumer").Get<LocalRabbitMQConsumerOptions>() ?? throw new Exception("RabbitMQ Consumer Options Not Found");
services.AddSingleton<IMessageQueueProcessor, CreateCustomerProcessor>();
services.AddSingleton<IMessageQueueProcessor, CreatePersonProfileLinkProcessor>();
services.AddSingleton<IMessageQueueProcessor, CreatePersonProfileProcessor>();
services.AddSingleton<IMessageQueueProcessor, CreateSupplierProcessor>();
services.AddSingleton<IMessageQueueProcessor, DeletePersonProfileAttachmentProcessor>();
services.AddSingleton<IMessageQueueProcessor, CreatePersonProfileProcessor>();
services.AddSingleton<IMessageQueueProcessor, ReadPersonProfileProcessor>();
services.AddSingleton<IMessageQueueProcessor, UpdateCustomerProcessor>();
services.AddSingleton<IMessageQueueProcessor, UpdatePersonProfileLinkProcessor>();
services.AddSingleton<IMessageQueueProcessor, UpdatePersonProfileProcessor>();
services.AddSingleton<IMessageQueueProcessor, UpdateSupplierProcessor>();
services.AddLocalRabbitMQConsumer(consumerOptions);

var producerOptions = configuration.GetSection("RabbitMQProducer").Get<LocalRabbitMQProducerOptions>() ?? throw new Exception("RabbitMQ producer Options Not Found");
services.AddLocalRabbitMQProducer(producerOptions);

var host = builder.Build();
host.Run();
