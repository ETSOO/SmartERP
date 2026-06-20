using com.etsoo.CoreFramework.Application;
using com.etsoo.MessageQueue.LocalRabbitMQ;
using com.etsoo.MessageQueue.QueueProcessors;
using com.etsoo.SendCloudSDK;
using com.etsoo.SMTP;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using PlatformShared.Database;
using PlatformShared.Services;
using WorkerCenter.Main.Processors;
using WorkerCenter.Periods;
using WorkerCenter.Workers;

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

// HTTP client
services.AddHttpClient();

// SMS client
var smsClientSection = configuration.GetSection("SMSClient");
if (!smsClientSection.Exists())
{
    throw new Exception("SMS configuration not found");
}
services.AddSendCloudClient(smsClientSection);

// SMTP client
var smtpOptions = configuration.GetSection("SMTPClient").Get<SMTPClientOptions>() ?? throw new Exception("SMTP configuration not found");
var smtpClient = new SMTPClient(smtpOptions);
services.AddSingleton<ISMTPClient>(smtpClient);

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

void OptionsAction(IServiceProvider provider, DbContextOptionsBuilder options)
{
    options.UseNpgsql(connectonString);

    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
}

// services.AddDbContextPool
services.AddDbContext<MyDbContext>(OptionsAction, ServiceLifetime.Singleton);

// Support DbContextFactory for multi-threaded scenarios, such as in background services or parallel processing
// It will also inject DbContext in scoped services
services.AddPooledDbContextFactory<MyDbContext>(OptionsAction);

services.AddDbContext<LogDbContext>((provider, options) =>
{
    options.UseNpgsql(logConnectionString);

    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
}, ServiceLifetime.Singleton);

services.Configure<SmartERPCoordinatorOptions>(configuration.GetSection("SmartERPCoordinator"));
services.AddSingleton<ISmartERPCoordinator, SmartERPCoordinator>();

var consumerOptions = configuration.GetSection("RabbitMQConsumer").Get<LocalRabbitMQConsumerOptions>() ?? throw new Exception("RabbitMQ Consumer Options Not Found");
services.AddSingleton<IMessageQueueProcessor, AcceptInvitationProcessor>();
services.AddSingleton<IMessageQueueProcessor, AddUserIdentifierProcessor>();
services.AddSingleton<IMessageQueueProcessor, AdjustReportToProcessor>();
services.AddSingleton<IMessageQueueProcessor, AdminClearUserFrozenProcessor>();
services.AddSingleton<IMessageQueueProcessor, AdminRenewAppProcessor>();
services.AddSingleton<IMessageQueueProcessor, AdminSupportProcessor>();
services.AddSingleton<IMessageQueueProcessor, BuyAppProcessor>();
services.AddSingleton<IMessageQueueProcessor, ChangePasswordProcessor>();
services.AddSingleton<IMessageQueueProcessor, CheckSessionProcessor>();
services.AddSingleton<IMessageQueueProcessor, CreateApiProcessor>();
services.AddSingleton<IMessageQueueProcessor, CreateApiKeyProcessor>();
services.AddSingleton<IMessageQueueProcessor, CreateDocumentProcessor>();
services.AddSingleton<IMessageQueueProcessor, CreateOrgProcessor>();
services.AddSingleton<IMessageQueueProcessor, CreateResourceProcessor>();
services.AddSingleton<IMessageQueueProcessor, DeleteDocumentProcessor>();
services.AddSingleton<IMessageQueueProcessor, DeleteMemberProcessor>();
services.AddSingleton<IMessageQueueProcessor, DeleteUserIdentifierProcessor>();
services.AddSingleton<IMessageQueueProcessor, GenerateDocumentProcessor>();
services.AddSingleton<IMessageQueueProcessor, LeaveOrgProcessor>();
services.AddSingleton<IMessageQueueProcessor, LoginFailedProcessor>();
services.AddSingleton<IMessageQueueProcessor, LoginSuccessProcessor>();
services.AddSingleton<IMessageQueueProcessor, RenewAppProcessor>();
services.AddSingleton<IMessageQueueProcessor, ResetPasswordProcessor>();
services.AddSingleton<IMessageQueueProcessor, SendAuthCodeEmailProcessor>();
services.AddSingleton<IMessageQueueProcessor, SendEmailProcessor>();
services.AddSingleton<IMessageQueueProcessor, SendSMSProcessor>();
services.AddSingleton<IMessageQueueProcessor, SwitchOrgProcessor>();
services.AddSingleton<IMessageQueueProcessor, UpdateApiProcessor>();
services.AddSingleton<IMessageQueueProcessor, UpdateAppProcessor>();
services.AddSingleton<IMessageQueueProcessor, UpdateDocumentProcessor>();
services.AddSingleton<IMessageQueueProcessor, UpdateMemberAvatarProcessor>();
services.AddSingleton<IMessageQueueProcessor, UpdateMemberProcessor>();
services.AddSingleton<IMessageQueueProcessor, UpdateOrgAvatarProcessor>();
services.AddSingleton<IMessageQueueProcessor, UpdateOrgProcessor>();
services.AddSingleton<IMessageQueueProcessor, UpdateUserAvatarProcessor>();
services.AddSingleton<IMessageQueueProcessor, UpdateUserSelfProcessor>();
services.AddLocalRabbitMQConsumer(consumerOptions);

var producerOptions = configuration.GetSection("RabbitMQProducer").Get<LocalRabbitMQProducerOptions>() ?? throw new Exception("RabbitMQ producer Options Not Found");
services.AddLocalRabbitMQProducer(producerOptions);

services.Configure<AssetCheckWorkerOptions>(configuration.GetSection("AssetCheckWorker"));
services.AddHostedService<AssetCheckWorker>();

services.Configure<AssetExiryWorkerOptions>(configuration.GetSection("AssetExiryWorker"));
services.AddHostedService<AssetExiryWorker>();

var host = builder.Build();
host.Run();
