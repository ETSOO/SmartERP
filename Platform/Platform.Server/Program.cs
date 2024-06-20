using com.etsoo.Web;
using com.etsoo.WeiXin;
using Platform.Server;
using Platform.Server.OAuth2;
using System.Text.Json.Serialization.Metadata;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

// Add services to the container.
var services = builder.Services;
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();
services.AddHttpClient();
services.ConfigureHttpJsonOptions(options =>
{
    // Use source generation
    options.SerializerOptions.TypeInfoResolver = JsonTypeInfoResolver.Combine(
        WeiXinJsonSerializerContext.Default,
        MyJsonSerializerContext.Default
    );
});

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

// Local services
services.Configure<WXClientOptions>(configuration.GetSection("WeiXin"));
services.AddTransient<IWXClient, WXClient>();

// API services


var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Enable CORS (Cross-Origin Requests)
// The call to UseCors must be placed after UseRouting, but before UseAuthorization
if (corsOptions.Required)
{
    app.UseCors();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
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
api.MapGroup("OAuth2")
    .MapGoogle()
    .MapWechat()
    .MapAlipay()
;

app.MapFallbackToFile("/index.html");

app.Run();