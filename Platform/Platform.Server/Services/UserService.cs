using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.HTTP;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Storage;
using Microsoft.EntityFrameworkCore;
using Platform.Server.Application;
using Platform.Server.Dto.App;
using Platform.Server.Dto.User;
using Platform.Server.Endpoints.AuthCode.RQ;
using Platform.Server.Endpoints.User.RQ;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using System.Buffers;
using System.Diagnostics;

namespace Platform.Server.Services
{
    /// <summary>
    /// User service
    /// 用户服务
    /// </summary>
    public class UserService : CommonUserService, IUserService
    {
        readonly MyDbContext _db;
        readonly LogDbContext _logDb;
        readonly IStorage _storage;
        readonly IAuthCodeService _authCodeService;

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="db">Database EF</param>
        /// <param name="app">Application</param>
        /// <param name="userAccessor">User accessor</param>
        /// <param name="logger">Logger</param>
        /// <param name="storage">Storage</param>
        /// <param name="authCodeService">AuthCode service</param>
        public UserService(MyDbContext db,
            LogDbContext logDb,
            IMyApp app,
            CurrentUserAccessor userAccessor,
            ILogger<UserService> logger,
            IStorage storage,
            IAuthCodeService authCodeService)
            : base(app, userAccessor.UserSafe, "user", logger)
        {
            _db = db;
            _logDb = logDb;
            _storage=storage;
            _authCodeService = authCodeService;
        }

        /// <summary>
        /// Add user email identifier
        /// 添加用户电子邮件标识
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="userAgent">User agent</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public async ValueTask<IActionResult> AddEmailAsync(CodeValidateRQ rq, string? userAgent, CancellationToken cancellationToken = default)
        {
            var (result, data) = _authCodeService.CreateValidateCodeData(rq, userAgent);
            if (!result.Ok || data == null)
            {
                return result;
            }

            var (resultValidate, resultData) = await _authCodeService.ValidateAsync(AuthCodeAction.UserVerificationEmailCode, data, cancellationToken);
            if (!resultValidate.Ok || resultData == null)
            {
                return resultValidate;
            }

            // Identifier
            var identifier = new CoreUserIdentifier
            {
                CoreUserId = User.IdInt,
                Type = CoreUserIdentifierType.Email,
                Value = resultData.OpenId.ToLower()
            };

            // Validate
            return await AddIdentifierAsync(identifier, cancellationToken);
        }

        /// <summary>
        /// Add user identifier
        /// 添加用户标识
        /// </summary>
        /// <param name="identifier">Identifier data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async ValueTask<IActionResult> AddIdentifierAsync(CoreUserIdentifier identifier, CancellationToken cancellationToken = default)
        {
            // Check the existence of the identifier
            var id = await _db.CoreUserIdentifiers.AsNoTracking()
                .Where(d => d.Type == identifier.Type && d.Value == identifier.Value)
                .Select(d => d.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (id > 0)
            {
                // Update
                await _db.CoreUserIdentifiers
                    .Where(d => d.Id == id)
                    .ExecuteUpdateAsync(d => d.SetProperty(d => d.CoreUserId, identifier.CoreUserId), cancellationToken);
            }
            else
            {
                // Add
                _db.CoreUserIdentifiers.Add(identifier);

                // Save changes
                await _db.SaveChangesAsync(cancellationToken);

                id = identifier.Id;
            }

            return ActionResult.Succeed(id);
        }

        /// <summary>
        /// Add user mobile identifier
        /// 添加用户移动电话标识
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="userAgent">User agent</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public async ValueTask<IActionResult> AddMobileAsync(CodeValidateRQ rq, string? userAgent, CancellationToken cancellationToken = default)
        {
            var (result, data) = _authCodeService.CreateValidateCodeData(rq, userAgent);
            if (!result.Ok || data == null)
            {
                return result;
            }

            var (resultValidate, resultData) = await _authCodeService.ValidateAsync(AuthCodeAction.UserVerificationSMSCode, data, cancellationToken);
            if (!resultValidate.Ok || resultData == null)
            {
                return resultValidate;
            }

            // Identifier
            var identifier = new CoreUserIdentifier
            {
                CoreUserId = User.IdInt,
                Type = CoreUserIdentifierType.Mobile,
                Value = resultData.OpenId
            };

            // Validate
            return await AddIdentifierAsync(identifier, cancellationToken);
        }

        /// <summary>
        /// All user identifiers
        /// 获取所有用户标识
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public Task<UserIdentifierData[]> AllIdentifiersAsync(CancellationToken cancellationToken = default)
        {
            return _db.CoreUserIdentifiers.AsNoTracking()
                .Where(d => d.CoreUserId == User.IdInt)
                .Select(d => new UserIdentifierData
                {
                    Id = d.Id,
                    Type = d.Type,
                    Value = MyDbFunctions.HideData(d.Value, '@'),
                    Creation = d.Creation
                })
                .ToArrayAsync(cancellationToken);
        }

        /// <summary>
        /// All user identifiers
        /// 获取所有用户标识
        /// </summary>
        /// <param name="writer">HTTP writer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task AllIdentifiersAsync(IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            await _db.CoreUserIdentifiers.AsNoTracking()
                .Where(d => d.CoreUserId == User.IdInt)
                .Select(d => new UserIdentifierData
                {
                    Id = d.Id,
                    Type = d.Type,
                    Value = MyDbFunctions.HideData(d.Value, '@'),
                    Creation = d.Creation
                })
                .ToJsonAsync(writer, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Audit history
        /// 操作历史
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="writer">JSON Writer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public async Task AuditHistoryAsync(AuditHistoryRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            var (hasContent, commandText) = await _logDb.CoreLogs.AsNoTracking()
                .Where(d => d.UserId == User.IdInt)
                .QueryEtsoo(rq, d => d.Id, null, (q) =>
                {
                    if (rq.Keyword?.Length > 1)
                    {
                        q = q.QueryEtsooKeywords(rq.Keyword, DbUtils.ILikeMethod, d => d.Title);
                    }

                    if (rq.DeviceId.HasValue)
                    {
                        q = q.Where(d => d.DeviceId == rq.DeviceId);
                    }

                    if (rq.Kind?.Length > 1)
                    {
                        q = q.Where(d => d.Kind == rq.Kind);
                    }

                    if (rq.CreationStart.HasValue)
                    {
                        q = q.Where(d => d.Creation >= rq.CreationStart);
                    }

                    if (rq.CreationEnd.HasValue)
                    {
                        q = q.Where(d => d.Creation < rq.CreationEnd);
                    }

                    return q;
                })
                .Select(d => new { d.Id, d.Kind, d.Title, d.Data, d.Culture, d.Creation })
                .ToJsonAsync(writer, cancellationToken: cancellationToken);

            if (_db.IsSensitiveDataLoggingEnabled)
            {
                Logger.LogInformation("AuditHistoryAsync is {hasContent} with {commandText}", hasContent, commandText);
            }
        }

        /// <summary>
        /// Delete user identifier
        /// 删除用户标识
        /// </summary>
        /// <param name="id">Identifier id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> DeleteIdentifierAsync(int id, CancellationToken cancellationToken = default)
        {
            var affacted = await _db.CoreUserIdentifiers.Where(d => d.Id == id
                    && d.CoreUserId == User.IdInt
                    && (d.Type > CoreUserIdentifierType.Mobile || _db.CoreUserIdentifiers.Any(sub => sub.CoreUserId == User.IdInt && sub.Type == d.Type && sub.Id != id)))
                .ExecuteDeleteAsync(cancellationToken);
            if (affacted == 0)
            {
                return ApplicationErrors.NoId.AsResult();
            }
            return ActionResult.Succeed(id);
        }

        /// <summary>
        /// Device list
        /// 设备列表
        /// </summary>
        /// <param name="rq">List query data</param>
        /// <param name="writer">JSON writer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public async Task DeviceListAsync(QueryIntRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            var (_, commandText) = await _db.CoreUserDevices.AsNoTracking()
                .Where(d => d.CoreUserId == User.IdInt)
                .QueryEtsoo(rq, d => d.Id, null, (q) =>
                {
                    if (rq.Keyword?.Length > 1)
                    {
                        q = q.Where(d => EF.Functions.ILike(d.Name, $"%{rq.Keyword}%"));
                    }

                    return q;
                })
                .Select(d => new { d.Id, d.Name })
                .ToJsonAsync(writer, cancellationToken: cancellationToken);

            Debug.WriteLine(commandText, nameof(DeviceListAsync));
        }

        /// <summary>
        /// Get user current appliations depends on token
        /// 基于令牌获取用户当前程序
        /// </summary>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>Result</returns>
        public async Task<IEnumerable<AppData>> GetCurrentAppsAsync(CancellationToken cancellationToken = default)
        {
            // User apps
            var ids = new List<int>
            {
                MyAppConstants.CoreAppId
            };

            if (User.Scopes != null)
            {
                // Super user
                if (User.Scopes.Contains(MyAppConstants.SuperApp)) ids.Add(MyAppConstants.SuperAppId);

                // Other apps
                foreach (var scope in User.Scopes)
                {
                    ids.Add(CurrentUser.ScopeToAppId(scope));
                }
            }

            // Private apps
            var apps = await _db.CoreApps.AsNoTracking()
                .GroupJoin(_db.CoreOrganizationApps, a => a.Id, oa => oa.CoreAppId, (a, oa) => new { a, oa })
                .SelectMany(t => t.oa.Where(oa => oa.CoreOrganizationId == User.OrganizationInt).DefaultIfEmpty(), (t, oa) => new AppData
                {
                    Id = t.a.Id,
                    Name = (oa == null || oa.LocalName == null) ? t.a.Name : oa.LocalName,
                    WebUrl = (oa == null || oa.LocalUrl == null) ? t.a.WebUrl : oa.LocalUrl,
                    HelpUrl = (oa == null || oa.LocalHelpUrl == null) ? t.a.HelpUrl : oa.LocalHelpUrl,
                    Logo = t.a.Logo
                })
               .ToArrayAsync(cancellationToken);

            return apps;
        }

        /// <summary>
        /// Get user's latest accessed appliation's Web URL
        /// 获取用户最近访问的程序的Web网址
        /// </summary>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>Web URL</returns>
        public async Task<string> GetLatestAppAsync(CancellationToken cancellationToken = default)
        {
            // Latest accessed app id
            var appId = User.AppId ?? MyAppConstants.CoreAppId;

            var url = await _db.CoreApps.AsNoTracking()
                .GroupJoin(_db.CoreOrganizationApps, a => a.Id, oa => oa.CoreAppId, (a, oa) => new { a, oa })
                .SelectMany(t => t.oa.Where(oa => oa.CoreOrganizationId == User.OrganizationInt).DefaultIfEmpty(), (t, oa) => oa == null ? t.a.WebUrl : oa.LocalUrl ?? t.a.WebUrl)
                .FirstOrDefaultAsync(cancellationToken);

            if (string.IsNullOrEmpty(url))
            {
                // Extreme case, get the core app url
                url = await _db.CoreApps.AsNoTracking()
                    .Where(a => a.Id == MyAppConstants.CoreAppId)
                    .Select(a => a.WebUrl)
                    .FirstAsync(cancellationToken);
            }

            return url;
        }

        /// <summary>
        /// Update avatar
        /// 更新头像
        /// </summary>
        /// <param name="avatarStream">Avatar stream</param>
        /// <param name="contentType">Content type</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>New URL</returns>
        public async ValueTask<IActionResult> UpdateAvatarAsync(Stream avatarStream, string contentType, CancellationToken cancellationToken = default)
        {
            // Check the stream
            if (avatarStream.Length is not > 10240 and < 102400000)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(avatarStream));
            }

            var extension = MimeTypeMap.TryGetExtension(contentType);
            if (string.IsNullOrEmpty(extension))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(contentType));
            }

            // File path
            var path = "/UserAvatar/" + DateTime.UtcNow.ToString("yyyyMM") + "/" + Path.GetRandomFileName() + extension;

            // Save the stream to file directly
            var saveResult = await _storage.WriteAsync(path, avatarStream, WriteCase.CreateNew, cancellationToken: cancellationToken);

            if (saveResult)
            {
                // New avatar URL
                var url = _storage.GetUrl(path);

                // Update
                await _db.CoreUsers.Where(u => u.Id == User.IdInt).ExecuteUpdateAsync(u => u.SetProperty(u => u.Avatar, url), cancellationToken);

                // Remove current avatar
                if (!string.IsNullOrEmpty(User.Avatar))
                    await _storage.DeleteUrlAsync(User.Avatar, cancellationToken);

                // Return
                return ActionResult.Succeed(url);
            }
            else
            {
                Logger.LogError("Avatar write path is {path}", path);
                return ApplicationErrors.DataProcessingFailed.AsResult();
            }
        }
    }
}
