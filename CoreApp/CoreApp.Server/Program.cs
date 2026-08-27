using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Authentication;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.ServiceApp.Application;
using com.etsoo.ServiceApp.Services;
using com.etsoo.ServiceApp.SmartERP;
using com.etsoo.Utils.Serialization;
using com.etsoo.Web;
using com.etsoo.WebUtils;
using CoreApp.Server;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

var isDevelopment = builder.Environment.IsDevelopment();

var configuration = builder.Configuration;

// Custom environment
var envName = Environment.GetEnvironmentVariable("ETSOO_ENVIRONMENT");
if (!string.IsNullOrEmpty(envName))
{
    configuration.AddJsonFile($"appsettings.{envName}.json", optional: true, reloadOnChange: true);
}

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

// Rate limiter
// https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit?view=aspnetcore-8.0
// https://blog.maartenballiauw.be/post/2022/09/26/aspnet-core-rate-limiting-middleware.html
var rateOptions = configuration.GetSection("RateLimiters/Etsoo").Get<EtsooRateLimiterOptions>();
services.AddRateLimiter(options =>
{
    var policy = new EtsooRateLimiterPolicy(rateOptions);
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context => policy.GetPartition(context));
    options.OnRejected = policy.OnRejected;
});

// Entity framework
var connectonString = configuration.GetConnectionString("SmartERP");
if (string.IsNullOrEmpty(connectonString))
{
    throw new Exception("SmartERP connection string not found");
}

// SmartERP Service Application
var seSection = configuration.GetSection("SmartERPService");
var seSettings = seSection.GetSection("Configuration").Get<ServiceAppConfiguration>();
var seJwt = seSection.GetSection("Jwt").Get<JwtSettings>();
if (seSettings == null || seJwt == null)
{
    throw new Exception("SmartERP Service Application configuration not found");
}
if (seSettings.Cultures.Length == 0)
{
    throw new Exception("SmartERP Service Application cultures not found");
}

var seApp = new SEServiceApp(services, new PostgreDatabase(connectonString), seSettings, appId: 1);
services.AddSingleton<ISEServiceApp>(seApp);

services.AddSingleton(seSettings);

// Adding Authentication in JwtService
var jwtService = new JwtService(services, seJwt, new JwtBearerEvents
{
    OnAuthenticationFailed = context =>
    {
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(context.Exception, "OnAuthenticationFailed");
        return Task.CompletedTask;
    }
});

services.AddSingleton<IAuthService>(jwtService);

// Localization cultures
var Cultures = seSettings.Cultures;
if (Cultures == null || Cultures.Length == 0)
{
    throw new Exception("No SmartERP Culture Defined");
}

// Authentication is the process of determining a user's identity.
// Authorization is the process of determining whether a user has access to a resource.
services.AddAuthorization();

services.AddHealthChecks();

// Add services to the container.
// services.AddAntiforgery(); // Only for cookie-based, but not needed for Token-based authentication
services.AddOpenApi();
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
        MyJsonSerializerContext.Default
    );
});

if (isDevelopment)
{
    // Development environment only
    // The remote certificate is invalid according to the validation procedure
    services.ConfigureHttpClientDefaults(builder =>
    {
        builder.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        });
    });
}

// Configue CORS
var cors = configuration.GetSection("Cors").Get<IEnumerable<string>?>()?.ToArray();
var corsOptions = new CorsPolicySetupOptions(cors, isDevelopment)
{
    ExposedHeaders = [Constants.RefreshTokenHeaderName, Constants.ContentDispositionHeaderName]
};

if (corsOptions.Required)
{
    services.AddCors(options =>
    {
        // Add default policy
        // Or AddPolicy with a specific policy
        options.AddDefaultPolicy(builder => builder.Setup(corsOptions));
    });
}

// API services
services.AddScoped<CurrentUserAccessor>();
services.AddScoped<ISEAuthService, SEAuthService>();

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
if (isDevelopment)
{
    app.UseDeveloperExceptionPage();
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "v1");
    });
}
else
{
    // Production
    app.UseHttpsRedirection();
}

// Request localization setup
// Use Content-Language Header for culture detection
// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/localization?view=aspnetcore-5.0
// https://www.jerriepelser.com/blog/how-aspnet5-determines-culture-info-for-localization/
var localizationOptions = new RequestLocalizationOptions
{
    ApplyCurrentCultureToResponseHeaders = true,
    RequestCultureProviders = [
        new QueryStringRequestCultureProvider(),
        new ContentLanguageHeaderRequestCultureProvider(),
        new AcceptLanguageHeaderRequestCultureProvider()
    ]
}.SetDefaultCulture(Cultures[0])
    .AddSupportedCultures(Cultures)
    .AddSupportedUICultures(Cultures);

app.UseRequestLocalization(localizationOptions);

// Rate limiter must be called after UseRouting, at least before UseAuthentication
app.UseRateLimiter();

app.MapHealthChecks("/healthz");

// APIs
var api = app.MapGroup("/api");

// Endpoints
api.MapAuth()
    .AddModelValidators()
    .RequireAuthorization()
;

app.MapFallbackToFile("/index.html");

try
{
    app.Run();
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "Error occurred during application ran");
}