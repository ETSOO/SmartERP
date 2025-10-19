using com.etsoo.ApiModel.Dto.SmartERP;
using com.etsoo.ApiModel.RQ.SmartERP;
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
using PlatformShared;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using PlatformShared.Extentions;
using PlatformShared.Messages;
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
        readonly IQueueService _queueService;
        readonly IPublicService _publicService;

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="db">Database EF</param>
        /// <param name="logDb">Log DB</param>
        /// <param name="app">Application</param>
        /// <param name="userAccessor">User accessor</param>
        /// <param name="logger">Logger</param>
        /// <param name="storage">Storage</param>
        /// <param name="authCodeService">AuthCode service</param>
        /// <param name="queueService">Queue service</param>
        /// <param name="publicService">Public service</param>
        public UserService(MyDbContext db,
            LogDbContext logDb,
            IMyApp app,
            CurrentUserAccessor userAccessor,
            ILogger<UserService> logger,
            IStorage storage,
            IAuthCodeService authCodeService,
            IQueueService queueService,
            IPublicService publicService)
            : base(app, userAccessor.UserSafe, "user", logger)
        {
            _db = db;
            _logDb = logDb;
            _storage=storage;
            _authCodeService = authCodeService;
            _queueService = queueService;
            _publicService = publicService;
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

            // Push message
            var message = new AddUserIdentifierMessage
            {
                Data = User.CreateMessageData(App.AppId, id),
                IdentifierType = identifier.Type,
                IdentifierValue = identifier.Value
            };
            await _queueService.PushAsync(message, PlatformSharedContext.Default.AddUserIdentifierMessage, cancellationToken);

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
            var orgId = User.OrganizationInt;
            var (hasContent, commandText) = await _logDb.CoreLogs.AsNoTracking()
                .Where(d => d.UserId == User.IdInt && (d.OrganizationId == null || d.OrganizationId == orgId))
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
                .Select(d => new
                {
                    d.Id,
                    d.Kind,
                    d.OrganizationId,
                    d.AppId,
                    d.Title,
                    d.Data,
                    d.Culture,
                    d.Creation
                })
                .ToJsonAsync(writer, cancellationToken: cancellationToken);

            if (_db.IsSensitiveDataLoggingEnabled)
            {
                Logger.LogInformation("AuditHistoryAsync is {hasContent} with {commandText}", hasContent, commandText);
            }
        }

        /// <summary>
        /// Check session
        /// 检查会话
        /// </summary>
        /// <param name="id">App id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> CheckSessionAsync(int id, CancellationToken cancellationToken = default)
        {
            // Check permission
            var app = CurrentUser.AppIdToScope(id);
            if (User.Scopes?.Contains(app) is not true)
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            var appName = id < 3 ? await _db.CoreApps.AsNoTracking()
                .Where(a => a.Id == id)
                .Select(a => a.Name)
                .FirstOrDefaultAsync(cancellationToken)
            : await _db.CoreOrganizationApps.AsNoTracking()
                .Where(a => a.CoreAppId == id && a.CoreOrganizationId == User.OrganizationInt)
                .Select(a => a.LocalName ?? a.CoreApp.Name)
                .FirstOrDefaultAsync(cancellationToken);

            if (string.IsNullOrEmpty(appName))
            {
                return ApplicationErrors.NoId.AsResult();
            }

            // Update latest apps
            var user = await _db.CoreUsers.Where(u => u.Id == User.IdInt).FirstOrDefaultAsync(cancellationToken);
            if (user == null)
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            if (user.LatestAppIds == null)
            {
                user.LatestAppIds = [id];
            }
            else
            {
                var ids = user.LatestAppIds;
                ids.Remove(id);
                if (ids.Count >= 10)
                {
                    ids.RemoveAt(ids.Count - 1);
                }
                user.LatestAppIds = [id, .. ids];
            }

            // Save changes
            await _db.SaveChangesAsync(cancellationToken);

            // Push message
            var message = new CheckSessionMessage
            {
                Data = User.CreateMessageData(id, 0, appName)
            };
            await _queueService.FirePushAsync(message, PlatformSharedContext.Default.CheckSessionMessage, cancellationToken);

            return ActionResult.Succeed(id);
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
            var data = await _db.CoreUserIdentifiers.Where(d => d.Id == id
                    && d.CoreUserId == User.IdInt
                    && (d.Type > CoreUserIdentifierType.Mobile || _db.CoreUserIdentifiers.Any(sub => sub.CoreUserId == User.IdInt && sub.Type == d.Type && sub.Id != id)))
                .Select(d => new { d.Type, d.Value })
                .FirstOrDefaultAsync(cancellationToken);

            if (data == null)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            await _db.CoreUserIdentifiers.Where(d => d.Id == id).ExecuteDeleteAsync(cancellationToken);

            // Push message
            var message = new DeleteUserIdentifierMessage
            {
                Data = User.CreateMessageData(App.AppId, id),
                IdentifierType = data.Type,
                IdentifierValue = data.Value
            };
            await _queueService.PushAsync(message, PlatformSharedContext.Default.DeleteUserIdentifierMessage, cancellationToken);

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
                        q = q.Where(d => EF.Functions.Like(d.Name, $"%{rq.Keyword}%"));
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
                // Admin user
                if (User.Scopes.Contains(MyAppConstants.AdminApp)) ids.Add(MyAppConstants.AdminAppId);

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
                    Urls = (oa == null || oa.LocalUrls == null) ? t.a.Urls : oa.LocalUrls,
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
        public async Task<AppData> GetLatestAppAsync(CancellationToken cancellationToken = default)
        {
            // Latest accessed app id
            var appId = User.AppId ?? MyAppConstants.CoreAppId;

            var app = await _db.CoreApps.AsNoTracking()
                .Where(a => a.Id == appId)
                .GroupJoin(_db.CoreOrganizationApps, a => a.Id, oa => oa.CoreAppId, (a, oa) => new { a, oa })
                .SelectMany(t => t.oa.Where(oa => oa.CoreOrganizationId == User.OrganizationInt).DefaultIfEmpty(), (t, oa) => new AppData
                {
                    Id = t.a.Id,
                    Name = (oa == null || oa.LocalName == null) ? t.a.Name : oa.LocalName,
                    Urls = (oa == null || oa.LocalUrls == null) ? t.a.Urls : oa.LocalUrls,
                    Logo = t.a.Logo
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (app == null)
            {
                // Extreme case, get the core app url
                app = await _db.CoreApps.AsNoTracking()
                    .Where(a => a.Id == MyAppConstants.CoreAppId)
                    .Select(a => new AppData
                    {
                        Id = a.Id,
                        Name = a.Name,
                        Urls = a.Urls,
                        Logo = a.Logo
                    })
                    .FirstAsync(cancellationToken);
            }

            return app;
        }

        /// <summary>
        /// Update user
        /// 更新用户
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> UpdateAsync(UserUpdateRQ rq, CancellationToken cancellationToken = default)
        {
            var u = await _db.CoreUsers.Where(u => u.Id == User.IdInt)
                .FirstOrDefaultAsync(cancellationToken);

            if (u == null)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            if (rq.IsModified(nameof(rq.Name)) && !string.IsNullOrEmpty(rq.Name))
            {
                u.Name = rq.Name;
                u.QueryKeyword = _publicService.GetPinyin(new PinyinRQ { Input = rq.Name, Format = PinyinFormatType.Initial });
            }

            if (rq.IsModified(nameof(rq.GivenName)))
            {
                u.GivenName = rq.GivenName;
            }

            if (rq.IsModified(nameof(rq.FamilyName)))
            {
                u.FamilyName = rq.FamilyName;
            }

            if (rq.IsModified(nameof(rq.LatinGivenName)))
            {
                u.LatinGivenName = rq.LatinGivenName;
            }

            if (rq.IsModified(nameof(rq.LatinFamilyName)))
            {
                u.LatinFamilyName = rq.LatinFamilyName;
            }

            if (rq.IsModified(nameof(rq.PreferredName)))
            {
                u.PreferredName = rq.PreferredName;
            }

            // Changes
            var changes = _db.ChangeTracker.Entries().GetChangedProperties();

            // Save
            await _db.SaveChangesAsync(cancellationToken);

            // Push message
            var message = new UpdateMemberMessage
            {
                Data = User.CreateMessageData(App.AppId, rq.Id),
                Changes = changes
            };
            await _queueService.PushAsync(message, PlatformSharedContext.Default.UpdateMemberMessage, cancellationToken);

            // Return
            return ActionResult.Succeed(rq.Id);
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

                // Push message
                var message = new UpdateUserAvatarMessage
                {
                    Data = User.CreateMessageData(App.AppId, 0)
                };
                await _queueService.PushAsync(message, PlatformSharedContext.Default.UpdateUserAvatarMessage, cancellationToken);

                // Return
                return ActionResult.Succeed(url);
            }
            else
            {
                Logger.LogError("Avatar write path is {path}", path);
                return ApplicationErrors.DataProcessingFailed.AsResult();
            }
        }

        /// <summary>
        /// Read user data for update
        /// 读取用于更新的用户数据
        /// </summary>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task UpdateReadAsync(IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            await _db.CoreUsers.AsNoTracking()
                .Where(u => u.Id == User.IdInt)
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.PreferredName,
                    u.FamilyName,
                    u.GivenName,
                    u.LatinFamilyName,
                    u.LatinGivenName
                }).ToJsonObjectAsync(writer, cancellationToken: cancellationToken);
        }
    }
}
