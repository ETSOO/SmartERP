using com.etsoo.Api.Helpers;
using com.etsoo.Core.Database;
using com.etsoo.SmartERP.Applications;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.IO.Compression;
using System.Text;

namespace com.etsoo.Api
{
    public class Startup
    {
        /// <summary>
        /// Configuration
        /// 配置对象
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// SmartERP Settings
        /// </summary>
        SmartERPSettings Settings { get; }

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="configuration">Configuration</param>
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;

            // SmartERP Settings
            Settings = Configuration.GetSection("AppSettings:SmartERP").Get<SmartERPSettings>();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Dependency Injection for the MainApp
            // https://stackoverflow.com/questions/38138100/addtransient-addscoped-and-addsingleton-services-differences
            // Add as singleton to enhance performance
            services.AddSingleton(new MainApp(
                // Configurations
                (configuration) =>
                {
                    configuration
                        .ModelValidated(true)
                        .SetKeys(Settings.PrivateKey, Settings.SymmetricKey)
                        .ServiceUser(Settings.ServiceUserId)
                    ;
                },

                // Database context
                new SqlServerDatabase(Configuration.GetConnectionString(Settings.ConnectionStringId))
            ));

            // Add to support access HttpContext
            services.AddHttpContextAccessor();

            // Configure distributed memory cache
            // https://dotnetcoretutorials.com/2017/03/05/using-inmemory-cache-net-core/
            services.AddDistributedMemoryCache();

            // Configue CORS
            if (Settings.Cors?.Length > 0)
            {
                services.AddCors(options =>
                {
                    options.AddPolicy("platform",
                    builder =>
                    {
                        builder.WithOrigins(Settings.Cors)
                            // Support https://*.domain.com
                            .SetIsOriginAllowedToAllowWildcardSubdomains()

                            // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Access-Control-Allow-Credentials
                            // https://stackoverflow.com/questions/24687313/what-exactly-does-the-access-control-allow-credentials-header-do
                            // JWT is not a cookie solution, disable it without allow credential
                            // .AllowCredentials()
                            .DisallowCredentials()

                            // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers
                            // Without it will popup error: Request header field content-type is not allowed by Access-Control-Allow-Headers in preflight response
                            .AllowAnyHeader()

                            // Web Verbs like GET, POST, default enabled
                            .AllowAnyMethod();
                    });
                });
            }

            // Configue compression
            // https://gunnarpeipman.com/aspnet-core-compress-gzip-brotli-content-encoding/
            services.Configure<BrotliCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Optimal;
            });

            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<BrotliCompressionProvider>();
            });

            // Configue JWT authentication
            // https://jwt.io/
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    // Is SSL only
                    options.RequireHttpsMetadata = Settings.SSL;

                    // Save token, True means tokens are cached in the server for validation
                    options.SaveToken = false;

                    // Token validation parameters
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuerSigningKey = true,
                        RequireExpirationTime = true,
                        ValidateLifetime = true,

                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Settings.SymmetricKey)),
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };
                });

            // Register controllers
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Enable HTTPS redirect
            if (Settings.SSL)
                app.UseHttpsRedirection();

            app.UseRouting();

            // Enable CORS (Cross-Origin Requests)
            // The call to UseCors must be placed after UseRouting, but before UseAuthorization
            if (Settings.Cors?.Length > 0)
            {
                app.UseCors("platform");
            }

            app.UseAuthentication();
            app.UseAuthorization();

            // Enable compression
            app.UseResponseCompression();

            app.UseEndpoints(endpoints =>
            {
                // Apply authentication by default
                endpoints.MapControllers().RequireAuthorization();
            });
        }
    }
}
