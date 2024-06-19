using com.etsoo.WeiXin;
using Platform.Server;
using Platform.Server.OAuth2;
using System.Text.Json.Serialization.Metadata;

var builder = WebApplication.CreateBuilder(args);

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

// Local services
services.Configure<WXClientOptions>(builder.Configuration.GetSection("WeiXin"));
services.AddTransient<IWXClient, WXClient>();

// API services


var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

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