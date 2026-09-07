using com.etsoo.ApiProxy.Defs;
using com.etsoo.ApiProxy.Options;
using com.etsoo.ApiProxy.Proxy;
using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Authentication;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.DI;
using com.etsoo.MessageQueue.LocalRabbitMQ;
using com.etsoo.ServiceApp.Services;
using com.etsoo.ServiceApp.SmartERP;
using com.etsoo.Utils.Serialization;
using com.etsoo.Web;
using com.etsoo.WebUtils;
using CRM.Server;
using CRM.Server.Application;
using CRM.Server.Endpoints;
using CRM.Server.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using PlatformShared.Database;
using PlatformShared.Extentions;
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

services.AddDbContext<MyDbContext>((provider, options) =>
{
    options.UseNpgsql(connectonString);

    if (isDevelopment)
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// SmartERP Service Application
var seSection = configuration.GetSection("SmartERPService");
var seSettings = seSection.GetSection("Configuration").Get<MyAppConfiguration>();
var seJwt = seSection.GetSection("Jwt").Get<JwtSettings>();
if (seSettings == null || seJwt == null)
{
    throw new Exception("SmartERP Service Application configuration not found");
}
if (seSettings.Cultures.Length == 0)
{
    throw new Exception("SmartERP Service Application cultures not found");
}

var seApp = new MyApp(services, new PostgreDatabase(connectonString), seSettings);
services.AddSingleton<IMyApp>(seApp);

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

// Fire and forget
services.AddSingleton<IFireAndForgetService, FireAndForgetService>();

// Add message queue
var mqOptions = configuration.GetSection("RabbitMQProducer").Get<LocalRabbitMQProducerOptions>() ?? throw new Exception("RabbitMQ producer configuration not found");
services.AddLocalRabbitMQProducer(mqOptions);

services.AddSingleton<IQueueService, QueueService>();

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

// SmartERP Core Proxy
services.AddOptions<SmartERPOptions>().Bind(configuration.GetSection("SmartERPProxy")).ValidateOnStart();
services.AddHttpClient<ISmartERPProxy, SmartERPProxy>(client =>
{
    var api = seSettings.ApiUrl;
    if (!api.EndsWith('/'))
    {
        api += '/';
    }
    client.BaseAddress = new Uri(api);
});

// API services
services.AddScoped<CurrentUserAccessor>();

// services.AddSEAuthService<IMyApp, MyAppConfiguration>();
services.AddScoped<ISEAuthService, CrmAuthService>();

services.AddScoped<ICommonService, CommonService>();
services.AddScoped<IAssetService, AssetService>();
services.AddScoped<ICustomerService, CustomerService>();
services.AddScoped<IDeptService, DeptService>();
services.AddScoped<IGroupService, GroupService>();
services.AddScoped<IOrderDeliveryService, OrderDeliveryService>();
services.AddScoped<IOrderPaymentService, OrderPaymentService>();
services.AddScoped<IOrderService, OrderService>();
services.AddScoped<IOrderLineService, OrderLineService>();
services.AddScoped<IPersonService, PersonService>();
services.AddScoped<IPersonAddressService, PersonAddressService>();
services.AddScoped<IPersonContactService, PersonContactService>();
services.AddScoped<IPersonCategoryService, PersonCategoryService>();
services.AddScoped<IPersonInfoService, PersonInfoService>();
services.AddScoped<IPersonProductService, PersonProductService>();
services.AddScoped<IPersonProfileService, PersonProfileService>();
services.AddScoped<IPOService, POService>();
services.AddScoped<IPOLineService, POLineService>();
services.AddScoped<IProductService, ProductService>();
services.AddScoped<IProductCategoryService, ProductCategoryService>();
services.AddScoped<IPromotionService, PromotionService>();
services.AddScoped<IStockService, StockService>();
services.AddScoped<IStockSiteService, StockSiteService>();
services.AddScoped<ISupplierService, SupplierService>();
services.AddScoped<ISystemService, SystemService>();
services.AddScoped<ITagService, TagService>();
services.AddScoped<IUserService, UserService>();

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
    .MapAsset()
    .MapCustomer()
    .MapDept()
    .MapGroup()
    .MapOrderDelivery()
    .MapOrderPayment()
    .MapOrder()
    .MapOrderLine()
    .MapPerson()
    .MapPersonAddress()
    .MapPersonContact()
    .MapPersonCategory()
    .MapPersonInfo()
    .MapPersonProduct()
    .MapPersonProfile()
    .MapPO()
    .MapPOLine()
    .MapProduct()
    .MapProductCategory()
    .MapPromotion()
    .MapStock()
    .MapStockSite()
    .MapSupplier()
    .MapSystem()
    .MapTag()
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