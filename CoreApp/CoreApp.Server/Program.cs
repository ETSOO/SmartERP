using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.ServiceApp.Application;
using com.etsoo.ServiceApp.Services;
using com.etsoo.ServiceApp.SmartERP;
using com.etsoo.Utils.Serialization;
using com.etsoo.Web;
using CoreApp.Server;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading.RateLimiting;

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
var seJwt = seSection.GetSection("Jwt").Get<com.etsoo.CoreFramework.Authentication.JwtSettings>();
if (seSettings == null || seJwt == null)
{
    throw new Exception("SmartERP Service Application configuration not found");
}
if (seSettings.Cultures.Length == 0)
{
    throw new Exception("SmartERP Service Application cultures not found");
}

var seApp = new SEServiceApp(services, seSettings, new PostgreDatabase(connectonString), seJwt);
services.AddSingleton<ISEServiceApp>(seApp);

// Authentication is the process of determining a user's identity.
// Authorization is the process of determining whether a user has access to a resource.
services.AddAuthorization();

// Add services to the container.
// services.AddAntiforgery(); // Only for cookie-based, but not needed for Token-based authentication
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
        MyJsonSerializerContext.Default
    );
});

// Configue CORS
var cors = configuration.GetSection("Cors").Get<IEnumerable<string>?>()?.ToArray();
var corsOptions = new CorsPolicySetupOptions(cors, builder.Environment.IsDevelopment())
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

// Rate limiter must be called after UseRouting, at least before UseAuthentication
app.UseRateLimiter();

// APIs
var api = app.MapGroup("/api").WithOpenApi();

// Endpoints
api.MapAuth()
    .AddModelValidators()
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