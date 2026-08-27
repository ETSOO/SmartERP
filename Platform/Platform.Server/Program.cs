using com.etsoo.AliAmapApi;
using com.etsoo.AlipayApi;
using com.etsoo.ApiModel;
using com.etsoo.ApiProxy.Configs;
using com.etsoo.ApiProxy.Defs;
using com.etsoo.ApiProxy.Proxy;
using com.etsoo.BaiduApi.Maps;
using com.etsoo.BaiduApi.Options;
using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.DI;
using com.etsoo.GarnetClient;
using com.etsoo.GoogleApi;
using com.etsoo.MessageQueue.LocalRabbitMQ;
using com.etsoo.MicrosoftApi;
using com.etsoo.ThirdPartyExtentions.Minio;
using com.etsoo.Utils.Serialization;
using com.etsoo.Utils.Storage;
using com.etsoo.Web;
using com.etsoo.WebUtils;
using com.etsoo.WeiXin;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using Platform.Server;
using Platform.Server.Application;
using Platform.Server.Endpoints.App;
using Platform.Server.Endpoints.Auth;
using Platform.Server.Endpoints.AuthCode;
using Platform.Server.Endpoints.Document;
using Platform.Server.Endpoints.Member;
using Platform.Server.Endpoints.Org;
using Platform.Server.Endpoints.Public;
using Platform.Server.Endpoints.Report;
using Platform.Server.Endpoints.Storage;
using Platform.Server.Endpoints.User;
using Platform.Server.OAuth2;
using Platform.Server.Services;
using PlatformShared;
using PlatformShared.Database;
using PlatformShared.Extentions;
using PlatformShared.Services;
using System.Globalization;
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

// Rate limiter
// https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit?view=aspnetcore-8.0
// https://blog.maartenballiauw.be/post/2022/09/26/aspnet-core-rate-limiting-middleware.html
services.AddRateLimiter(options =>
{
    var globalRateOptions = configuration.GetSection("RateLimiters:Global").Get<EtsooRateLimiterOptions>();
    var globalPolicy = new EtsooRateLimiterPolicy(globalRateOptions);
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context => globalPolicy.GetPartition(context));
    options.OnRejected = globalPolicy.OnRejected;

    var piiRateOptions = configuration.GetSection("RateLimiters:PII").Get<EtsooRateLimiterOptions>();
    var piiPolicy = new EtsooRateLimiterPolicy(piiRateOptions);
    options.AddPolicy("PII", piiPolicy);
});

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
//services.AddDbContext<MyDbContext>(OptionsAction);
// It will also inject DbContext in scoped services
services.AddPooledDbContextFactory<MyDbContext>((IServiceProvider provider, DbContextOptionsBuilder options) =>
{
    options.UseNpgsql(connectonString);

    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

services.AddDbContext<LogDbContext>((provider, options) =>
{
    options.UseNpgsql(logConnectionString);

    if (isDevelopment)
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// SmartERP
var erpSection = configuration.GetSection("SmartERP");
var erpSettings = erpSection.GetSection("Configuration").Get<MyAppConfiguration>();
var erpJwt = erpSection.GetSection("Jwt").Get<com.etsoo.CoreFramework.Authentication.JwtSettings>();
if (erpSettings == null || erpJwt == null)
{
    throw new Exception("SmartERP configuration not found");
}
if (erpSettings.Cultures.Length == 0)
{
    throw new Exception("SmartERP cultures not found");
}

/*
new JwtBearerEvents
{
    OnAuthenticationFailed = context =>
    {
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogWarning("OnAuthenticationFailed {context}", context);
        return Task.CompletedTask;
    },
    OnTokenValidated = context =>
    {
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
        var claims = context.Principal?.Claims.Select(claim => $"{claim.Type} = {claim.Value}");
        var claimsString = claims == null ? null : string.Join(", ", claims);
        logger.LogWarning("OnTokenValidated {IsAuthenticated} with {claims}", context.Principal?.Identity?.IsAuthenticated, claimsString);
        return Task.CompletedTask;
    },
    OnChallenge = context =>
    {
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogWarning("OnChallenge {context}", context);
        return Task.CompletedTask;
    }
}
*/

var erp = new MyApp(services, erpSettings.PrivateKey, new PostgreDatabase(connectonString));
services.AddSingleton<IMyApp>(erp);

// Adding Authentication in JwtService
var jwtService = new com.etsoo.CoreFramework.Authentication.JwtService(services, erpJwt, new JwtBearerEvents
{
    OnAuthenticationFailed = context =>
    {
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(context.Exception, "OnAuthenticationFailed");
        return Task.CompletedTask;
    }
});

services.AddSingleton<com.etsoo.CoreFramework.Authentication.IAuthService>(jwtService);

// Localization cultures
var Cultures = erpSettings.Cultures;
if (Cultures == null || Cultures.Length == 0)
{
    throw new Exception("No SmartERP Culture Defined");
}

var healthBuilder = services.AddHealthChecks()
    .AddNpgSql(connectonString);

// Storage
var storageS3Section = erpSection.GetSection("StorageS3");
if (storageS3Section.Exists())
{
    services.AddS3StorageClient(storageS3Section);
    healthBuilder.AddS3Storage();
}
else
{
    var storageOptions = erpSection.GetSection("Storage").Get<StorageOptions>() ?? throw new Exception("Storage configuration not found");
    var storage = new LocalStorage(storageOptions);
    services.AddSingleton<IStorage>(storage);
    healthBuilder.AddLocalStorage();
}

// Bridge Proxy APIs
services.Configure<BridgeOptions>(configuration.GetSection(BridgeOptions.SectionName));
services.AddHttpClient<IBridgeProxy, BridgeProxy>();

// Baidu APIs
services.Configure<MapsOptions>(configuration.GetSection("BaiduMaps"));
services.AddHttpClient<IMapPlaceService, MapPlaceService>();

// Amap APIs
services.Configure<AmapOptions>(configuration.GetSection("Amap"));
services.AddHttpClient<IAmapService, AmapService>();

// Authentication is the process of determining a user's identity.
// Authorization is the process of determining whether a user has access to a resource.
services.AddAuthorization();

// Configure Json serialization
services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;

    // Use source generation
    options.SerializerOptions.TypeInfoResolver = JsonTypeInfoResolver.Combine(
        CommonJsonSerializerContext.Default,
        ModelJsonSerializerContext.Default,
        ApiModelJsonSerializerContext.Default,
        WeiXinJsonSerializerContext.Default,
        PlatformSharedContext.Default,
        MyJsonSerializerContext.Default
    );
});

// Add services to the container.
services.AddAntiforgery(); // Only for cookie-based, but not needed for Token-based authentication
services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        // Ensure instances exist
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

        document.Components.SecuritySchemes["Bearer"] =
            new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = JwtBearerDefaults.AuthenticationScheme.ToLower(),
                BearerFormat = "JWT",
                Description = "Input your JWT token"
            };

        var securityRequirement = new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        };

        document.Security ??= [];
        document.Security.Add(securityRequirement);

        return Task.CompletedTask;
    });
});
services.AddHttpClient();
services.AddHttpContextAccessor();

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

// Add message queue
var mqOptions = configuration.GetSection("RabbitMQProducer").Get<LocalRabbitMQProducerOptions>() ?? throw new Exception("RabbitMQ producer configuration not found");
services.AddLocalRabbitMQProducer(mqOptions);

services.AddSingleton<IQueueService, QueueService>();

// Configue compression
// https://gunnarpeipman.com/aspnet-core-compress-gzip-brotli-content-encoding/
/*
services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Optimal;
});

services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
});
*/

// Configue CORS
// Cors for internal (SmartERP) APIs
// PublicCors for public (Custom applications) APIs
var cors = configuration.GetSection("Cors").Get<IEnumerable<string>?>()?.ToArray();
var publicCors = configuration.GetSection("PublicCors").Get<IEnumerable<string>?>()?.ToArray();
var corsOptions = new CorsPolicySetupOptions(cors, isDevelopment)
{
    ExposedHeaders = [Constants.RefreshTokenHeaderName, Constants.ContentDispositionHeaderName]
};
var publicCorsOptions = new CorsPolicySetupOptions(publicCors, false)
{
    ExposedHeaders = [Constants.RefreshTokenHeaderName]
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

// Auth2 clients
var authClients = erpSettings.AuthClients;
int authClientsFlag = 0;

if (authClients.Length > 0)
{
    foreach (var client in authClients)
    {
        // Options
        var options = configuration.GetSection($"{client}Auth");
        if (!options.Exists())
        {
            continue;
        }

        switch (client.ToLower())
        {
            case "Wechat":
                services.AddWechatAuthClient(options);
                authClientsFlag |= 1;
                break;
            case "Alipay":
                services.AddAlipayClient(options);
                authClientsFlag |= 2;
                break;
            case "Google":
                services.AddGoogleAuthClient(options);
                authClientsFlag |= 4;
                break;
            case "Microsoft":
                services.AddMicrosoftAuthClient(options);
                authClientsFlag |= 8;
                break;
        }
    }
}

services.AddSingleton(Options.Create(new SmartERPCoordinatorOptions
{
    PrivateKey = erpSettings.PrivateKey
}));
services.AddScoped<ISmartERPCoordinator, SmartERPCoordinator>();

services.AddScoped<IStorageFactory, StorageFactory>();

// Local services
services.Configure<WXClientOptions>(configuration.GetSection("WeiXin"));
services.AddScoped<IWXClient, WXClient>();

// API services
services.AddScoped<CurrentUserAccessor>();
services.AddSingleton(erpSettings);
services.AddScoped<IAppService, AppService>();
services.AddScoped<IAuthService, AuthService>();
services.AddScoped<IAuthCodeService, AuthCodeService>();
services.AddScoped<IDocumentService, DocumentService>();
services.AddScoped<IMemberService, MemberService>();
services.AddScoped<IOrgService, OrgService>();
services.AddScoped<IPublicService, PublicService>();
services.AddScoped<IReportService, ReportService>();
services.AddScoped<IStorageService, StorageService>();
services.AddScoped<IUserService, UserService>();

var app = builder.Build();

app.UseForwardedHeaders();

app.UseDefaultFiles();
app.UseStaticFiles();

// Enable compression
// app.UseResponseCompression();

// Enable CORS (Cross-Origin Requests)
// The call to UseCors must be placed after UseRouting, but before UseAuthorization
if (corsOptions.Required)
{
    app.UseCors();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

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

// OAuth2 integration
if (authClientsFlag > 0)
{
    var oauth = api.MapGroup("OAuth2").AllowAnonymous();

    if ((authClientsFlag & 1) != 0)
    {
        oauth.MapWechat();
    }

    // Alipay
    if ((authClientsFlag & 2) != 0)
    {
        oauth.MapAlipay();
    }

    // Google
    if ((authClientsFlag & 4) != 0)
    {
        oauth.MapGoogle();
    }

    // Microsoft
    if ((authClientsFlag & 8) != 0)
    {
        oauth.MapMicrosoft();
    }
}

// Endpoints
api.MapAuth()
    .MapAuthCode()
    .MapApp()
    .MapDocument()
    .MapMember()
    .MapOrg()
    .MapPublic()
    .MapReport()
    .MapStorage()
    .MapUser()
    .AddModelValidators()
    .RequireAuthorization()
;

app.MapFallbackToFile("/index.html");

try
{
    app.Run();

    app.Logger.LogWarning("Current culture is {culture}, {nativeName}", CultureInfo.CurrentCulture.Name, CultureInfo.CurrentCulture.NativeName);
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "Error occurred during application ran");
}