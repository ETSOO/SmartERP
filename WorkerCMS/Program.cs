using com.etsoo.CoreFramework.Application;
using com.etsoo.MessageQueue.LocalRabbitMQ;
using com.etsoo.MessageQueue.QueueProcessors;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using PlatformShared.Database;
using WorkerCMS.Processors.Order;
using WorkerCMS.Processors.Org;
using WorkerCMS.Processors.Person;
using WorkerCMS.Processors.PO;
using WorkerCMS.Processors.Product;
using WorkerCMS.Processors.Stock;

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
var openTelemetryBuilder = services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService(builder.Environment.ApplicationName))
    .WithLogging((logging) =>
    {
        logging.AddConsoleExporter()
            .AddOtlpExporter((options) =>
            {
                options.Protocol = otlpExportOptions.Logging.Protocol ?? otlpExportOptions.Protocol;
                options.Endpoint = otlpExportOptions.Logging.Endpoint;
                options.Headers = otlpExportOptions.Logging.Headers ?? otlpExportOptions.Headers;
            });
    });

if (otlpExportOptions.Metrics != null)
{
    openTelemetryBuilder.WithMetrics((builder) =>
    {
        builder.AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation()

            .AddOtlpExporter((options) =>
            {
                options.Protocol = otlpExportOptions.Metrics.Protocol ?? otlpExportOptions.Protocol;
                options.Endpoint = otlpExportOptions.Metrics.Endpoint;
                options.Headers = otlpExportOptions.Metrics.Headers ?? otlpExportOptions.Headers;
            });
    });
}

if (otlpExportOptions.Tracing != null)
{
    openTelemetryBuilder.WithTracing((builder) =>
    {
        builder.SetSampler(new TraceIdRatioBasedSampler(0.1))
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddSource("Npgsql")

            .AddOtlpExporter((options) =>
            {
                options.Protocol = otlpExportOptions.Tracing.Protocol ?? otlpExportOptions.Protocol;
                options.Endpoint = otlpExportOptions.Tracing.Endpoint;
                options.Headers = otlpExportOptions.Tracing.Headers ?? otlpExportOptions.Headers;
            });
    });
}

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
services.AddPooledDbContextFactory<MyDbContext>((provider, options) =>
{
    options.UseNpgsql(connectonString);

    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

services.AddPooledDbContextFactory<LogDbContext>((provider, options) =>
{
    options.UseNpgsql(logConnectionString);

    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

var consumerOptions = configuration.GetSection("RabbitMQConsumer").Get<LocalRabbitMQConsumerOptions>() ?? throw new Exception("RabbitMQ Consumer Options Not Found");

// Order
services.AddSingleton<IMessageQueueProcessor, CreateOrderProcessor>();
services.AddSingleton<IMessageQueueProcessor, ReadOrderProcessor>();
services.AddSingleton<IMessageQueueProcessor, RecalculateOrderProcessor>();
services.AddSingleton<IMessageQueueProcessor, UpdateOrderProcessor>();

services.AddSingleton<IMessageQueueProcessor, CompleteOrderLineProcessor>();
services.AddSingleton<IMessageQueueProcessor, CreateOrderLineProcessor>();
services.AddSingleton<IMessageQueueProcessor, DeleteOrderLineProcessor>();
services.AddSingleton<IMessageQueueProcessor, ReadOrderLineProcessor>();
services.AddSingleton<IMessageQueueProcessor, RollbackOrderLineProcessor>();
services.AddSingleton<IMessageQueueProcessor, StartOrderLineProcessor>();
services.AddSingleton<IMessageQueueProcessor, UpdateOrderLineProcessor>();

services.AddSingleton<IMessageQueueProcessor, CreateOrderDeliveryProcessor>();
services.AddSingleton<IMessageQueueProcessor, SortOrderDeliveryProcessor>();
services.AddSingleton<IMessageQueueProcessor, UpdateOrderDeliveryProcessor>();

services.AddSingleton<IMessageQueueProcessor, CreateOrderPaymentProcessor>();
services.AddSingleton<IMessageQueueProcessor, SortOrderPaymentProcessor>();
services.AddSingleton<IMessageQueueProcessor, UpdateOrderPaymentProcessor>();

// PO
services.AddSingleton<IMessageQueueProcessor, CreatePOProcessor>();
services.AddSingleton<IMessageQueueProcessor, ReadPOProcessor>();
services.AddSingleton<IMessageQueueProcessor, RecalculatePOProcessor>();
services.AddSingleton<IMessageQueueProcessor, UpdatePOProcessor>();

services.AddSingleton<IMessageQueueProcessor, CompletePOLineProcessor>();
services.AddSingleton<IMessageQueueProcessor, CreatePOLineProcessor>();
services.AddSingleton<IMessageQueueProcessor, DeletePOLineProcessor>();
services.AddSingleton<IMessageQueueProcessor, ReadPOLineProcessor>();
services.AddSingleton<IMessageQueueProcessor, RollbackPOLineProcessor>();
services.AddSingleton<IMessageQueueProcessor, StartPOLineProcessor>();
services.AddSingleton<IMessageQueueProcessor, UpdatePOLineProcessor>();

// Org
services.AddSingleton<IMessageQueueProcessor, CreateAssetProcessor>();
services.AddSingleton<IMessageQueueProcessor, ReadAssetSensitiveDataProcessor>();
services.AddSingleton<IMessageQueueProcessor, UpdateAssetProcessor>();

services.AddSingleton<IMessageQueueProcessor, CreateDeptProcessor>();
services.AddSingleton<IMessageQueueProcessor, UpdateDeptProcessor>();

services.AddSingleton<IMessageQueueProcessor, UpdateCultureProcessor>();
services.AddSingleton<IMessageQueueProcessor, UpdateSettingsProcessor>();

services.AddSingleton<IMessageQueueProcessor, UpdateUserProcessor>();

// Person
services.AddSingleton<IMessageQueueProcessor, CreatePersonAddressProcessor>();
services.AddSingleton<IMessageQueueProcessor, CreatePersonLocationProcessor>();
services.AddSingleton<IMessageQueueProcessor, DeletePersonAddressProcessor>();
services.AddSingleton<IMessageQueueProcessor, UpdatePersonAddressProcessor>();

services.AddSingleton<IMessageQueueProcessor, CreatePersonCategoryProcessor>();
services.AddSingleton<IMessageQueueProcessor, MergePersonCategoryProcessor>();
services.AddSingleton<IMessageQueueProcessor, SortPersonCategoryProcessor>();
services.AddSingleton<IMessageQueueProcessor, UpdatePersonCategoryProcessor>();

services.AddSingleton<IMessageQueueProcessor, CreatePersonInfoProcessor>();
services.AddSingleton<IMessageQueueProcessor, DeletePersonInfoProcessor>();
services.AddSingleton<IMessageQueueProcessor, UpdatePersonInfoProcessor>();

services.AddSingleton<IMessageQueueProcessor, AddContactRelationProcessor>();
services.AddSingleton<IMessageQueueProcessor, CreateContactProcessor>();
services.AddSingleton<IMessageQueueProcessor, DeleteContactRelationProcessor>();
services.AddSingleton<IMessageQueueProcessor, UpdateContactRelationProcessor>();

services.AddSingleton<IMessageQueueProcessor, CreateCustomerProcessor>();
services.AddSingleton<IMessageQueueProcessor, CreateSupplierProcessor>();
services.AddSingleton<IMessageQueueProcessor, DeletePersonProcessor>();
services.AddSingleton<IMessageQueueProcessor, ReadPersonProcessor>();
services.AddSingleton<IMessageQueueProcessor, UpdateCustomerProcessor>();
services.AddSingleton<IMessageQueueProcessor, UpdatePersonProcessor>();
services.AddSingleton<IMessageQueueProcessor, UpdateSupplierProcessor>();

services.AddSingleton<IMessageQueueProcessor, CreatePersonProductProcessor>();
services.AddSingleton<IMessageQueueProcessor, DeletePersonProductProcessor>();
services.AddSingleton<IMessageQueueProcessor, UpdatePersonProductProcessor>();

services.AddSingleton<IMessageQueueProcessor, CreatePersonProfileLinkProcessor>();
services.AddSingleton<IMessageQueueProcessor, CreatePersonProfileProcessor>();
services.AddSingleton<IMessageQueueProcessor, DeletePersonProfileAttachmentProcessor>();
services.AddSingleton<IMessageQueueProcessor, CreatePersonProfileProcessor>();
services.AddSingleton<IMessageQueueProcessor, ReadPersonProfileProcessor>();
services.AddSingleton<IMessageQueueProcessor, UpdatePersonProfileLinkProcessor>();
services.AddSingleton<IMessageQueueProcessor, UpdatePersonProfileProcessor>();

// Product
services.AddSingleton<IMessageQueueProcessor, CreateProductProcessor>();
services.AddSingleton<IMessageQueueProcessor, DeleteProductProcessor>();
services.AddSingleton<IMessageQueueProcessor, ProductEditBomsProcessor>();
services.AddSingleton<IMessageQueueProcessor, UpdateProductProcessor>();
services.AddSingleton<IMessageQueueProcessor, UpdateProductLogoProcessor>();
services.AddSingleton<IMessageQueueProcessor, UpdateProductPriceProcessor>();
services.AddSingleton<IMessageQueueProcessor, UpdateProductUnitProcessor>();

services.AddSingleton<IMessageQueueProcessor, CreateProductCategoryProcessor>();
services.AddSingleton<IMessageQueueProcessor, MergeProductCategoryProcessor>();
services.AddSingleton<IMessageQueueProcessor, SortProductCategoryProcessor>();
services.AddSingleton<IMessageQueueProcessor, UpdateProductCategoryProcessor>();

services.AddSingleton<IMessageQueueProcessor, CreatePromotionProcessor>();
services.AddSingleton<IMessageQueueProcessor, SortPromotionProcessor>();
services.AddSingleton<IMessageQueueProcessor, UpdatePromotionProcessor>();

// Stock
services.AddSingleton<IMessageQueueProcessor, DeleteStockProcessor>();
services.AddSingleton<IMessageQueueProcessor, ReadStockProcessor>();
services.AddSingleton<IMessageQueueProcessor, StockAssembleProcessor>();
services.AddSingleton<IMessageQueueProcessor, StockCreateLineProcessor>();
services.AddSingleton<IMessageQueueProcessor, StockInitProcessor>();
services.AddSingleton<IMessageQueueProcessor, StockLoseProcessor>();
services.AddSingleton<IMessageQueueProcessor, StockOrderOutProcessor>();
services.AddSingleton<IMessageQueueProcessor, StockPOInProcessor>();
services.AddSingleton<IMessageQueueProcessor, StockReceiveProcessor>();
services.AddSingleton<IMessageQueueProcessor, StockTakeProcessor>();
services.AddSingleton<IMessageQueueProcessor, StockTransferProcessor>();
services.AddSingleton<IMessageQueueProcessor, UpdateStockLineProcessor>();
services.AddSingleton<IMessageQueueProcessor, UpdateStockProcessor>();

services.AddLocalRabbitMQConsumer(consumerOptions);

var producerOptions = configuration.GetSection("RabbitMQProducer").Get<LocalRabbitMQProducerOptions>() ?? throw new Exception("RabbitMQ producer Options Not Found");
services.AddLocalRabbitMQProducer(producerOptions);

var host = builder.Build();
host.Run();
