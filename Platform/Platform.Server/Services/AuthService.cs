using com.etsoo.ApiModel.Auth;
using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Business;
using com.etsoo.CoreFramework.Models;
using com.etsoo.HTTP;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Storage;
using com.etsoo.Web;
using com.etsoo.WebUtils;
using Microsoft.EntityFrameworkCore;
using Platform.Server.Application;
using Platform.Server.Database;
using Platform.Server.Database.Models;
using Platform.Server.Dto.Auth;
using System.Web;

namespace Platform.Server.Services
{
    public class AuthService : CommonService, IAuthService
    {
        private readonly MyDbContext _db;
        private readonly IStorage _storage;
        private readonly IHttpClientFactory _httpClientFactory;

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="userAccessor">User accessor</param>
        /// <param name="logger">Logger</param>
        /// <param name="storage">Storage</param>
        /// <param name="httpClientFactory">HTTP client factory</param>
        public AuthService(MyDbContext db, IMyApp app, IMyUserAccessor userAccessor, ILogger<AuthService> logger, IStorage storage, IHttpClientFactory httpClientFactory)
            : base(app, userAccessor.User, "auth", logger)
        {
            _db = db;
            _storage = storage;
            _httpClientFactory=httpClientFactory;
        }

        /// <summary>
        /// Get log in URL
        /// 获取登录URL
        /// </summary>
        /// <param name="client">Auth client</param>
        /// <param name="userAgent">User agent</param>
        /// <param name="deviceId">Device id</param>
        /// <returns>Result</returns>
        public IResult GetLogInUrl(IAuthClient client, string? userAgent, string deviceId)
        {
            if (!this.CheckDevice(userAgent, deviceId, out var result, out _))
            {
                return Results.BadRequest(result);
            }
            else
            {
                return Results.Content(client.GetLogInUrl(deviceId), "text/plain");
            }
        }

        /// <summary>
        /// Get sign up URL
        /// 获取注册URL
        /// </summary>
        /// <param name="client">Auth client</param>
        /// <param name="userAgent">User agent</param>
        /// <param name="deviceId">Device id</param>
        /// <returns>Result</returns>
        public IResult GetSignUpUrl(IAuthClient client, string? userAgent, string deviceId)
        {
            if (!this.CheckDevice(userAgent, deviceId, out var result, out _))
            {
                return Results.BadRequest(result);
            }
            else
            {
                return Results.Content(client.GetSignUpUrl(deviceId), "text/plain");
            }
        }

        /// <summary>
        /// Log in from OAuth2 client
        /// 从OAuth2客户端登录
        /// </summary>
        /// <param name="client">OAuth2 client</param>
        /// <param name="type">Auth type</param>
        /// <param name="context">OAuth2 Request HTTPContext</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async ValueTask LogInAsync(IAuthClient client, CoreUserIdentifierType type, HttpContext context, CancellationToken cancellationToken = default)
        {
            var (result, userInfo) = await client.GetUserInfoAsync(context.Request, (s) => this.CheckDevice(context.UserAgent(), s.Replace(" ", "+")), null, cancellationToken);

            if (result.Ok && userInfo != null)
            {
                var loginUser = await ReadUserAsync(type, userInfo.OpenId);
                if (loginUser == null)
                {
                    var url = $"{App.Configuration.AuthFailureUrl}?type={type}";
                    context.Response.Redirect(url);
                }
                else
                {
                    ValidateUser(context.Response, type, loginUser);
                }
            }
            else
            {
                Logger.LogError("Log in failed: {result}", result);
                RedirectToFailureUrl(context.Response, type, result.Title ?? "Log in failed", result.Type, result.Field);
            }
        }

        private async Task<LoginUser?> ReadUserAsync(CoreUserIdentifierType type, string openId)
        {
            return await _db.CoreUserIdentifiers.Where(i => i.Type == type && i.Value == openId)
                .Select(i => new LoginUser { Status = i.CoreUser.Status, FrozenTime = i.CoreUser.FrozenTime, Step = i.CoreUser.Step })
                .FirstOrDefaultAsync();
        }

        private void RedirectToFailureUrl(HttpResponse response, CoreUserIdentifierType type, string error, string? errorType = null, string? errorField = null)
        {
            var url = $"{App.Configuration.AuthFailureUrl}?type={type}&error={HttpUtility.UrlEncode(error)}&errorType={HttpUtility.UrlEncode(errorType)}&errorField={HttpUtility.UrlEncode(errorField)}";
            response.Redirect(url);
        }

        private void ValidateUser(HttpResponse response, CoreUserIdentifierType type, LoginUser user)
        {
            if (user.FrozenTime.HasValue)
            {
                var error = string.Format(ApplicationErrors.UserFrozen.Title, user.FrozenTime.ToString());
                RedirectToFailureUrl(response, type, error, ApplicationErrors.UserFrozen.Type);
            }
            else if (user.Status > EntityStatus.Approved)
            {
                var error = ApplicationErrors.AccountDisabled.Title;
                RedirectToFailureUrl(response, type, error, ApplicationErrors.AccountDisabled.Type);
            }
            else if (user.Step > 0)
            {
                var url = $"{App.Configuration.AuthRegistrationUrl}{user.Step}";
                response.Redirect(url);
            }
            else
            {
                response.Redirect(App.Configuration.AuthSuccessUrl);
            }
        }

        /// <summary>
        /// Sign up from OAuth2 client
        /// 从OAuth2客户端注册
        /// </summary>
        /// <param name="client">OAuth2 client</param>
        /// <param name="type">Auth type</param>
        /// <param name="context">OAuth2 Request HTTPContext</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async ValueTask SignUpAsync(IAuthClient client, CoreUserIdentifierType type, HttpContext context, CancellationToken cancellationToken = default)
        {
            var (result, userInfo) = await client.GetUserInfoAsync(context.Request, (s) => this.CheckDevice(context.UserAgent(), s.Replace(" ", "+")), null, cancellationToken);

            if (result.Ok && userInfo != null)
            {
                var loginUser = await ReadUserAsync(type, userInfo.OpenId);
                if (loginUser == null)
                {
                    string? avatar = null;
                    if (!string.IsNullOrEmpty(userInfo.Picture))
                    {
                        // Download avatar
                        try
                        {
                            using var response = await _httpClientFactory.CreateClient().GetAsync(userInfo.Picture, cancellationToken);
                            response.EnsureSuccessStatusCode();
                            var ext = MimeTypeMap.TryGetExtension(response.Content.Headers.ContentType?.MediaType) ?? Path.GetExtension(userInfo.Picture);
                            var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                            if (stream != null)
                            {
                                var avatarPath = $"/UserAvatars/{DateTime.UtcNow:yyyyMM}/{userInfo.OpenId}{ext}";
                                var tags = new Dictionary<string, string>() { { "AuthType", type.ToString() }, { "UserId", userInfo.OpenId } };
                                var saveResult = await _storage.WriteAsync(avatarPath, stream, WriteCase.CreateNew, tags, cancellationToken);
                                if (saveResult)
                                {
                                    avatar = _storage.GetUrl(avatarPath);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // Only log the exception, no block for execution
                            Logger.LogError(ex, "Download avatar failed: {url}", userInfo.Picture);
                        }
                    }

                    var hasEmail = !string.IsNullOrEmpty(userInfo.Email) && userInfo.EmailVerified is true;

                    // Basic data
                    var user = new CoreUser
                    {
                        Name = userInfo.Name,
                        FamilyName = userInfo.FamilyName,
                        GivenName = userInfo.GivenName,
                        Avatar = avatar,
                        Step = hasEmail ? CoreUserStep.Email : CoreUserStep.OAuth
                    };

                    // Current oauth data
                    user.CoreUserIdentifiers.Add(new CoreUserIdentifier
                    {
                        Type = type,
                        Value = userInfo.OpenId,
                    });

                    if (hasEmail)
                    {
                        // Additional email data
                        user.CoreUserIdentifiers.Add(new CoreUserIdentifier
                        {
                            Type = CoreUserIdentifierType.Email,
                            Value = userInfo.Email!,
                            RefType = type
                        });
                    }

                    await _db.CoreUsers.AddAsync(user, cancellationToken);
                    await _db.SaveChangesAsync(cancellationToken);

                    ValidateUser(context.Response, type, new LoginUser { Status = user.Status, FrozenTime = user.FrozenTime, Step = user.Step });
                }
                else
                {
                    ValidateUser(context.Response, type, loginUser);
                }
            }
            else
            {
                Logger.LogError("Sign up failed: @{result}", result);
                RedirectToFailureUrl(context.Response, type, result.Title ?? "Sign up failed", result.Type, result.Field);
            }
        }

        /// <summary>
        /// Web init call
        /// Web初始化调用
        /// </summary>
        /// <param name="rq">Rquest data</param>
        /// <param name="identifier">User identifier</param>
        /// <returns>Result</returns>
        public async ValueTask<IActionResult> WebInitCallAsync(InitCallRQ rq, string identifier)
        {
            // Init call
            return await InitCallAsync(rq, identifier);
        }

        /// <summary>
        /// Async init call update
        /// 异步初始化调用更新
        /// </summary>
        /// <param name="prevDeviceId">Previous client device id</param>
        /// <param name="newDeviceId">New client device id</param>
        /// <param name="deviceId">Serverside device id</param>
        /// <returns>Task</returns>
        protected override async Task InitCallUpdateAsync(string prevDeviceId, string newDeviceId, int deviceId)
        {
            await Task.CompletedTask;
        }
    }
}