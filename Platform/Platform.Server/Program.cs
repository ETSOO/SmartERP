using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.User;
using com.etsoo.DI;
using com.etsoo.GarnetClient;
using com.etsoo.Utils.Serialization;
using com.etsoo.Web;
using com.etsoo.WeiXin;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using Platform.Server;
using Platform.Server.Application;
using Platform.Server.Endpoints.Auth;
using Platform.Server.Endpoints.Public;
using Platform.Server.OAuth2;
using Platform.Server.Services;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

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

/*
 * Metrics with OpenTelemetry
otBuilder
    .WithMetrics(builder => builder
        .AddRuntimeInstrumentation()
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter((exporterOptions, metricReaderOptions) =>
        {
            exporterOptions.Endpoint = new Uri("http://localhost:9090/api/v1/otlp/v1/metrics");
            exporterOptions.Protocol = OtlpExportProtocol.HttpProtobuf;
            metricReaderOptions.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds = 1000;
        })
    );
*/

services.AddAuthentication().AddJwtBearer();
services.AddAuthorization();

// Add services to the container.
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();
services.AddHttpClient();
services.AddHttpContextAccessor();
services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;

    // Use source generation
    options.SerializerOptions.TypeInfoResolver = JsonTypeInfoResolver.Combine(
        ModelJsonSerializerContext.Default,
        CommonJsonSerializerContext.Default,
        WeiXinJsonSerializerContext.Default,
        MyJsonSerializerContext.Default
    );
});

// Rate limiter
// https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit?view=aspnetcore-8.0

// Cache
var redis = configuration.GetConnectionString("SmartERPRedis");
if (!string.IsNullOrEmpty(redis))
{
    services.AddGarnetCache(options =>
    {
        options.Configuration = redis;
    });
}
else
{
    services.AddDistributedMemoryCache();
}

// Fire and forget
services.AddSingleton<IFireAndForgetService, FireAndForgetService>();

// Configue CORS
// Cors for internal (SmartERP) APIs
// PublicCors for public (Custom applications) APIs
var cors = configuration.GetSection("Cors").Get<IEnumerable<string>?>()?.ToArray();
var publicCors = configuration.GetSection("PublicCors").Get<IEnumerable<string>?>()?.ToArray();
var corsOptions = new CorsPolicySetupOptions(cors, builder.Environment.IsDevelopment())
{
    ExposedHeaders = [""]
};
var publicCorsOptions = new CorsPolicySetupOptions(publicCors, false)
{
    ExposedHeaders = [""]
};

services.AddCors(options =>
{
    if (corsOptions.Required)
    {
        // Add default policy
        options.AddDefaultPolicy(builder => builder.Setup(corsOptions));
    }

    if (publicCorsOptions.Required)
    {
        // Add public policy
        options.AddPolicy("PublicCors", builder => builder.Setup(publicCorsOptions));
    }
});

// Entity framework

// SmartERP
var erpSection = configuration.GetSection("SmartERP");
var erpSettings = erpSection.GetSection("Configuration").Get<MyAppConfiguration>();

if (erpSettings == null)
{
    throw new NullReferenceException(nameof(erpSettings));
}

var erpJwt = erpSection.GetSection("Jwt").Get<com.etsoo.CoreFramework.Authentication.JwtSettings>();

services.AddSingleton<IMyApp>((provider) =>
{
    var factory = provider.GetRequiredService<ILoggerFactory>();
    return new MyApp(services, erpSettings, null, erpJwt, new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            factory.CreateLogger("OnAuthenticationFailed").LogError(context.Exception, "JWT OnAuthentication Failed");
            return Task.CompletedTask;
        }
    });
});

// Local services
services.Configure<WXClientOptions>(configuration.GetSection("WeiXin"));
services.AddScoped<IWXClient, WXClient>();

// API services
services.AddScoped<IMyUserAccessor, UserAccessor<CurrentUser>>();
services.AddScoped<IAuthService, AuthService>();
services.AddScoped<IPublicService, PublicService>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Enable CORS (Cross-Origin Requests)
// The call to UseCors must be placed after UseRouting, but before UseAuthorization
if (corsOptions.Required)
{
    app.UseCors();
}

app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // Production
    app.UseHttpsRedirection();
}

// APIs
var api = app.MapGroup("/api").WithOpenApi();

// OAuth2 integration
api.MapGroup("OAuth2").AllowAnonymous()
    .MapGoogle()
    .MapWechat()
    .MapAlipay()
;

api.MapAuth()
    .MapPublic()
;

app.MapFallbackToFile("/index.html");

app.Run();