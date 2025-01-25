using com.etsoo.AlipayApi;
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
using com.etsoo.MicrosoftApi;
using com.etsoo.SendCloudSDK;
using com.etsoo.SMTP;
using com.etsoo.ThirdPartyExtentions.Minio;
using com.etsoo.Utils.Serialization;
using com.etsoo.Utils.Storage;
using com.etsoo.Web;
using com.etsoo.WeiXin;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using Platform.Server;
using Platform.Server.Application;
using Platform.Server.Endpoints.App;
using Platform.Server.Endpoints.Auth;
using Platform.Server.Endpoints.AuthCode;
using Platform.Server.Endpoints.Member;
using Platform.Server.Endpoints.Org;
using Platform.Server.Endpoints.Public;
using Platform.Server.Endpoints.Storage;
using Platform.Server.Endpoints.User;
using Platform.Server.OAuth2;
using Platform.Server.Services;
using PlatformShared.Database;
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

// services.AddDbContextPool
services.AddDbContext<MyDbContext>((provider, options) =>
{
    options.UseNpgsql(connectonString);

    if (builder.Environment.IsDevelopment())
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

var erp = new MyApp(services, erpSettings, new PostgreDatabase(connectonString), erpJwt);
services.AddSingleton<IMyApp>(erp);

// It's done by JwtService of MyApp
// services.AddAuthentication().AddJwtBearer();

// SMS client
var smsClientSection = erpSection.GetSection("SMS");
if (!smsClientSection.Exists())
{
    throw new Exception("SMS configuration not found");
}
services.AddSendCloudClient(smsClientSection);

// SMTP client
var smtpOptions = erpSection.GetSection("SMTP").Get<SMTPClientOptions>() ?? throw new Exception("SMTP configuration not found");
var smtpClient = new SMTPClient(smtpOptions);
services.AddSingleton<ISMTPClient>(smtpClient);

// Storage
var storageS3Section = erpSection.GetSection("StorageS3");
if (storageS3Section.Exists())
{
    services.AddS3StorageClient(storageS3Section);
}
else
{
    var storageOptions = erpSection.GetSection("Storage").Get<StorageOptions>() ?? throw new Exception("Storage configuration not found");
    var storage = new LocalStorage(storageOptions);
    services.AddSingleton<IStorage>(storage);
}

// Bridge Proxy APIs
services.Configure<BridgeOptions>(configuration.GetSection(BridgeOptions.SectionName));
services.AddHttpClient<IBridgeProxy, BridgeProxy>();

// Baidu APIs
services.Configure<MapsOptions>(configuration.GetSection("BaiduMaps"));
services.AddHttpClient<IMapPlaceService, MapPlaceService>();

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
        WeiXinJsonSerializerContext.Default,
        MyJsonSerializerContext.Default
    );
});

// Add services to the container.
services.AddAntiforgery();
services.AddEndpointsApiExplorer();
services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            []
        }
    });
});
services.AddHttpClient();
services.AddHttpContextAccessor();

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
var wechatOptions = configuration.GetSection("WechatAuth");
if (wechatOptions.Exists())
{
    services.AddWechatAuthClient(wechatOptions);
}

var alipayOptions = configuration.GetSection("AlipayAuth");
if (alipayOptions.Exists())
{
    services.AddAlipayClient(alipayOptions);
}

var googleOptions = configuration.GetSection("GoogleAuth");
if (googleOptions != null)
{
    services.AddGoogleAuthClient(googleOptions);
}

var microsoftOptions = configuration.GetSection("MicrosoftAuth");
if (microsoftOptions != null)
{
    services.AddMicrosoftAuthClient(microsoftOptions);
}

// Local services
services.Configure<WXClientOptions>(configuration.GetSection("WeiXin"));
services.AddScoped<IWXClient, WXClient>();

// API services
services.AddScoped<CurrentUserAccessor>();
services.AddScoped<IAppService, AppService>();
services.AddScoped<IAuthService, AuthService>();
services.AddScoped<IAuthCodeService, AuthCodeService>();
services.AddScoped<IMemberService, MemberService>();
services.AddScoped<IOrgService, OrgService>();
services.AddScoped<IPublicService, PublicService>();
services.AddScoped<IUserService, UserService>();

var app = builder.Build();

app.UseForwardedHeaders();

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

app.UseAntiforgery();

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

// OAuth2 integration
var oauth = api.MapGroup("OAuth2").AllowAnonymous();

if (wechatOptions.Exists())
{
    oauth.MapWechat();
}

// Alipay
if (alipayOptions.Exists())
{
    oauth.MapAlipay();
}

// Google
if (googleOptions.Exists())
{
    oauth.MapGoogle();
}

// Microsoft
if (microsoftOptions.Exists())
{
    oauth.MapMicrosoft();
}

// Endpoints
api.MapAuth()
    .MapAuthCode()
    .MapApp()
    .MapMember()
    .MapOrg()
    .MapPublic()
    .MapStorage()
    .MapUser()
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