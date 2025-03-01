using Admin.Server.Dto.Query;
using Admin.Server.RQ.Query;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.ServiceApp.SmartERP;
using com.etsoo.Utils.Serialization;
using Microsoft.EntityFrameworkCore;
using PlatformShared.Database;
using System.Buffers;
using System.Net;

namespace Admin.Server.Services
{
    /// <summary>
    /// Query service
    /// 查询服务
    /// </summary>
    public class QueryService : SEUserService, IQueryService
    {
        readonly MyDbContext _db;
        readonly LogDbContext _logDb;

        public QueryService(
            MyDbContext db,
            LogDbContext logDb,
            ISEServiceApp app,
            CurrentUserAccessor userAccessor,
            ILogger<QueryService> logger
        )
            : base(app, userAccessor.UserSafe, "query", logger)
        {
            _db = db;
            _logDb = logDb;
        }

        /// <summary>
        /// Query all apps
        /// 查询所有应用
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="writer">JSON Writer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public async Task AllAppAsync(AllAppRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            var (hasContent, commandText) =  await _db.CoreOrganizationApps
                .AsNoTracking()
                .QueryEtsoo(rq, oa => oa.Id, oa => oa.Status, (q) =>
                {
                    if (rq.IdentityType.HasValue)
                    {
                        q = q.Where(oa => oa.CoreApp.IdentityType == rq.IdentityType);
                    }

                    if (rq.AppId.HasValue)
                    {
                        q = q.Where(oa => oa.CoreAppId == rq.AppId);
                    }

                    if (rq.OrgId.HasValue)
                    {
                        q = q.Where(oa => oa.CoreOrganizationId == rq.OrgId);
                    }

                    if (rq.Keyword?.Length > 1)
                    {
                        var keyword = rq.Keyword;

                        q = q.QueryEtsooKeywords(keyword, DbUtils.ILikeMethod, oa => oa.LocalName ?? oa.CoreApp.Name);
                    }

                    if (rq.Expiry.HasValue)
                    {
                        q = q.Where(oa => oa.Expiry < rq.Expiry && oa.Expiry >= DateTimeOffset.UtcNow);
                    }

                    if (rq.ExpiryDays.HasValue)
                    {
                        var expiryDays = rq.ExpiryDays.Value;
                        q = q.Where(oa => oa.Expiry < DateTimeOffset.UtcNow.AddDays(expiryDays) && oa.Expiry >= DateTimeOffset.UtcNow.AddDays(-expiryDays));
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
                }).Select(oa => new
                {
                    oa.Id,
                    oa.CoreApp.Name,
                    oa.LocalName,
                    oa.CoreApp.IdentityType,
                    OrgName = oa.CoreOrganization.Name,
                    oa.Status,
                    oa.Expiry,
                    ExpiryDays = oa.Expiry == null ? null : (int?)(oa.Expiry.Value - DateTimeOffset.UtcNow).TotalDays,
                    oa.Creation
                })
                .ToJsonAsync(writer, cancellationToken: cancellationToken);

            if (_db.IsSensitiveDataLoggingEnabled)
            {
                Logger.LogInformation("GetPurchasedAppsAsync is {hasContent} with {commandText}", hasContent, commandText);
            }
        }

        /// <summary>
        /// Query all organizations
        /// 查询所有机构
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="writer">JSON Writer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public async Task AllOrgAsync(AllOrgRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            var (hasContent, commandText) = await _db.CoreOrganizations
                .AsNoTracking()
                .QueryEtsoo(rq, o => o.Id, o => o.Status, (q) =>
                {
                    if (rq.ParentId.HasValue)
                    {
                        q = q.Where(o => o.ParentId == rq.ParentId);
                    }

                    if (rq.OwnerId.HasValue)
                    {
                        q = q.Where(o => o.OwnerId == rq.OwnerId);
                    }

                    if (rq.Pin?.Length > 1)
                    {
                        q = q.Where(o => o.Pin != null && EF.Functions.ILike(o.Pin, $"%{rq.Pin}%"));
                    }

                    if (rq.CreationStart.HasValue)
                    {
                        q = q.Where(o => o.Creation >= rq.CreationStart);
                    }

                    if (rq.CreationEnd.HasValue)
                    {
                        q = q.Where(o => o.Creation < rq.CreationEnd);
                    }

                    if (rq.Keyword?.Length > 1)
                    {
                        var keyword = rq.Keyword;

                        if (keyword.IsComplexQueryKeywords())
                        {
                            q = q.QueryEtsooKeywords(keyword, DbUtils.ILikeMethod, o => o.Name);
                        }
                        else
                        {
                            q = q.Where(o => o.Brand == keyword
                            || EF.Functions.ILike(o.Name, $"%{keyword}%")
                            || (o.Pin != null && EF.Functions.ILike(o.Pin, $"%{keyword}%"))
                            || (o.QueryKeyword != null && EF.Functions.ILike(o.QueryKeyword, $"%{keyword}%")));
                        }
                    }

                    return q;
                })
                .Select(o => new
                {
                    o.Id,
                    o.Name,
                    o.Brand,
                    Apps = o.CoreOrganizationApps.Count(),
                    Users = o.CoreOrganizationUsers.Count(),
                    Owner = o.Owner.Name,
                    Pin = MyDbFunctions.HideData(o.Pin, default),
                    o.Status,
                    o.Creation
                })
                .ToJsonAsync(writer, cancellationToken: cancellationToken);

            if (_db.IsSensitiveDataLoggingEnabled)
            {
                Logger.LogInformation("AllOrgAsync is {hasContent} with {commandText}", hasContent, commandText);
            }
        }

        /// <summary>
        /// Query all users
        /// 查询所有用户
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="writer">JSON Writer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public async Task AllUserAsync(AllUserRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            var (hasContent, commandText) = await _db.CoreUsers
                .AsNoTracking()
                .QueryEtsoo(rq, u => u.Id, u => u.Status, (q) =>
                {
                    if (rq.IsFrozen.HasValue)
                    {
                        if (rq.IsFrozen.Value)
                        {
                            q = q.Where(u => u.FrozenTime >= DateTime.UtcNow);
                        }
                        else
                        {
                            q = q.Where(u => u.FrozenTime == null || u.FrozenTime < DateTime.UtcNow);
                        }
                    }

                    if (rq.Keyword?.Length > 0)
                    {
                        var keyword = rq.Keyword;
                        q = q.Where(u => u.LatinGivenName == keyword
                        || u.QueryKeyword == keyword
                        || EF.Functions.ILike(u.Name, $"%{keyword}%")
                        || u.PreferredName == null || EF.Functions.ILike(u.PreferredName, $"%{keyword}%"));
                    }

                    if (rq.CreationStart.HasValue)
                    {
                        q = q.Where(u => u.Creation >= rq.CreationStart);
                    }

                    if (rq.CreationEnd.HasValue)
                    {
                        q = q.Where(u => u.Creation < rq.CreationEnd);
                    }

                    if (rq.Identifier?.Length >= 5)
                    {
                        q = q.Where(u => u.CoreUserIdentifiers.Any(i => i.Value == rq.Identifier));
                    }

                    if (rq.OrgId.HasValue)
                    {
                        q = q.Where(u => u.CoreOrganizationUsers.Any(ou => ou.CoreOrganizationId == rq.OrgId.Value));
                    }

                    if (rq.InviterId.HasValue)
                    {
                        q = q.Where(u => u.CoreOrganizationUsers.Any(ou => ou.InviterId == rq.InviterId.Value));
                    }

                    if (rq.Pin?.Length > 1)
                    {
                        q = q.Where(u => u.Pin != null && EF.Functions.ILike(u.Pin, $"%{rq.Pin}%"));
                    }

                    return q;
                })
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.PreferredName,
                    Pin = MyDbFunctions.HideData(u.Pin, default),
                    u.Status,
                    u.Creation,

                    Orgs = u.CoreOrganizationUsers.Count(),
                })
                .ToJsonAsync(writer, cancellationToken: cancellationToken);

            if (_db.IsSensitiveDataLoggingEnabled)
            {
                Logger.LogInformation("AllUserAsync is {hasContent} with {commandText}", hasContent, commandText);
            }
        }

        /// <summary>
        /// App list
        /// 应用列表
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="writer">Writer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public async Task AppListAsync(AppListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            var (hasContent, commandText) = await _db.CoreApps
                .AsNoTracking()
                .QueryEtsoo(rq, a => a.Id, null, (q) =>
                {
                    if (rq.Keyword?.Length > 0)
                    {
                        if (int.TryParse(rq.Keyword, out int id))
                        {
                            q = q.Where(a => a.Id == id);
                        }
                        else
                        {
                            q = q.QueryEtsooKeywords(rq.Keyword, DbUtils.ILikeMethod, a => a.Name);
                        }
                    }

                    if (rq.Enabled.HasValue)
                    {
                        q = q.Where(a => a.Enabled == rq.Enabled);
                    }

                    return q;
                })
                .Select(a => new
                {
                    a.Id,
                    a.Name
                })
                .ToJsonAsync(writer, cancellationToken: cancellationToken);

            if (_db.IsSensitiveDataLoggingEnabled)
            {
                Logger.LogInformation("AppListAsync is {hasContent} with {commandText}", hasContent, commandText);
            }
        }

        /// <summary>
        /// Organization list
        /// 机构列表
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="writer">Writer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public async Task OrgListAsync(OrgListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            var (hasContent, commandText) = await _db.CoreOrganizations
                .AsNoTracking()
                .QueryEtsoo(rq, o => o.Id, o => o.Status, (q) =>
                {
                    if (rq.Keyword?.Length > 0)
                    {
                        var keyword = rq.Keyword;
                        if (int.TryParse(keyword, out int id))
                        {
                            q = q.Where(o => o.Id == id);
                        }
                        else
                        {
                            q = q.Where(o => o.Brand == keyword
                            || EF.Functions.ILike(o.Name, $"%{keyword}%")
                            || (o.Pin != null && EF.Functions.ILike(o.Pin, $"%{keyword}%"))
                            || (o.QueryKeyword != null && EF.Functions.ILike(o.QueryKeyword, $"%{keyword}%")));
                        }
                    }

                    return q;
                })
                .Select(o => new
                {
                    o.Id,
                    o.Name,
                    Pin = MyDbFunctions.HideData(o.Pin, default),
                })
                .ToJsonAsync(writer, cancellationToken: cancellationToken);

            if (_db.IsSensitiveDataLoggingEnabled)
            {
                Logger.LogInformation("OrgListAsync is {hasContent} with {commandText}", hasContent, commandText);
            }
        }

        /// <summary>
        /// User list
        /// 用户列表
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="writer">Writer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public async Task UserListAsync(UserListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            var (hasContent, commandText) = await _db.CoreUsers
                .AsNoTracking()
                .QueryEtsoo(rq, u => u.Id, null, (q) =>
                {
                    if (rq.OrgId.HasValue)
                    {
                        q = q.Where(u => u.CoreOrganizationUsers.Any(ou => ou.CoreUserId == u.Id && ou.CoreOrganizationId == rq.OrgId.Value));
                    }

                    if (rq.ExcludeSelf is true)
                    {
                        q = q.Where(u => u.Id != User.IdInt);
                    }

                    if (rq.Keyword?.Length > 0)
                    {
                        if (int.TryParse(rq.Keyword, out int id))
                        {
                            q = q.Where(u => u.Id == id);
                        }
                        else
                        {
                            q = q.QueryEtsooKeywords(rq.Keyword, DbUtils.ILikeMethod, u => u.Name, u => u.PreferredName);
                        }
                    }

                    return q;
                })
                .Select(u => new
                {
                    u.Id,
                    u.Name
                })
                .ToJsonAsync(writer, cancellationToken: cancellationToken);

            if (_db.IsSensitiveDataLoggingEnabled)
            {
                Logger.LogInformation("UserListAsync is {hasContent} with {commandText}", hasContent, commandText);
            }
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
                .QueryEtsoo(rq, d => d.Id, null, (q) =>
                {
                    if (rq.Keyword?.Length > 1)
                    {
                        q = q.QueryEtsooKeywords(rq.Keyword, DbUtils.ILikeMethod, d => d.Title);
                    }

                    if (rq.UserId.HasValue)
                    {
                        q = q.Where(d => d.UserId == rq.UserId);
                    }

                    if (rq.OrgId.HasValue)
                    {
                        q = q.Where(d => d.OrganizationId == rq.OrgId);
                    }

                    if (rq.AppId.HasValue)
                    {
                        q = q.Where(d => d.AppId == rq.AppId);
                    }

                    if (rq.TargetId.HasValue)
                    {
                        q = q.Where(d => d.TargetId == rq.TargetId);
                    }

                    if (rq.Kind?.Length > 1)
                    {
                        q = q.Where(d => d.Kind == rq.Kind);
                    }

                    if (rq.Ip?.Length > 1)
                    {
                        q = q.Where(d => d.Ip == IPAddress.Parse(rq.Ip));
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
                d.Title,
                d.Data,
                d.Culture,
                d.Ip,
                d.UserId,
                d.OrganizationId,
                d.TargetId,
                d.AppId,
                d.Creation
            })
            .ToJsonAsync(writer, cancellationToken: cancellationToken);

            if (_db.IsSensitiveDataLoggingEnabled)
            {
                Logger.LogInformation("AuditHistoryAsync is {hasContent} with {commandText}", hasContent, commandText);
            }
        }

        /// <summary>
        /// Read app data
        /// 读取应用数据
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="writer">Writer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task ReadAppAsync(int id, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            await _db.CoreOrganizationApps
                .AsNoTracking()
                .Where(oa => oa.Id == id)
                .Select(oa => new
                {
                    oa.Id,
                    oa.CoreApp.Name,
                    oa.LocalName,
                    oa.LocalUrls,
                    oa.CoreApp.IdentityType,
                    OrgId = oa.CoreOrganizationId,
                    OrgName = oa.CoreOrganization.Name,
                    AppId = oa.CoreAppId,
                    oa.CoreApp.Urls,
                    oa.Status,
                    oa.Expiry,
                    oa.AppKey,
                    ExpiryDays = oa.Expiry == null ? null : (int?)(oa.Expiry.Value - DateTimeOffset.UtcNow).TotalDays,
                    oa.Creation
                })
                .ToJsonObjectAsync(writer, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Read organization data
        /// 读取机构数据
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="writer">Writer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task ReadOrgAsync(int id, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            await _db.CoreOrganizations
                .AsNoTracking()
                .Where(o => o.Id == id)
                .Select(o => new
                {
                    o.Id,
                    o.Name,
                    o.OwnerId,
                    OwnerName = o.Owner.Name,
                    o.Brand,
                    o.Logo,
                    o.Pin,
                    o.QueryKeyword,
                    o.ParentId,
                    ParentName = o.Parent == null ? null : o.Parent.Name,
                    o.Region,
                    o.Status,
                    o.Creation,

                    Apps = o.CoreOrganizationApps.Count(),
                    Users = o.CoreOrganizationUsers.Count()
                })
                .ToJsonObjectAsync(writer, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Read user data
        /// 读取用户数据
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<ReadUserDto?> ReadUserAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _db.CoreUsers
                .AsNoTracking()
                .Where(u => u.Id == id)
                .Select(u => new ReadUserDto
                {
                    Id = u.Id,
                    Avatar = u.Avatar,
                    Pin = MyDbFunctions.HideData(u.Pin, default),
                    FamilyName = u.FamilyName,
                    GivenName = u.GivenName,
                    LatinFamilyName = u.LatinFamilyName,
                    LatinGivenName = u.LatinGivenName,
                    PreferredName = u.PreferredName,
                    Name = u.Name,
                    Status = u.Status,
                    Creation = u.Creation,
                    FrozenTime = u.FrozenTime,

                    Orgs = u.CoreOrganizations.Count(),
                    OrgList = u.CoreOrganizations.OrderByDescending(d => d.Id).Select(o => new IdNameItem
                    {
                        Id = o.Id,
                        Name = o.Name
                    }).Take(6),
                    Devices = u.CoreUserDevices.Count(),
                    DeviceList = u.CoreUserDevices.OrderByDescending(d => d.Id).Select(d => new IdNameItem
                    {
                        Id = d.Id,
                        Name = d.Name
                    }).Take(6),
                    IdentifierList = u.CoreUserIdentifiers.OrderByDescending(d => d.Id).Select(i => new UserIdentifierItem
                    {
                        Id = i.Id,
                        Type = i.Type,
                        Value = MyDbFunctions.HideData(i.Value, default)
                    }).Take(6)
                })
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Read user data
        /// 读取用户数据
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="writer">Writer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task ReadUserAsync(int id, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            var user = await ReadUserAsync(id, cancellationToken);
            await writer.SerializeAsync(user, MyJsonSerializerContext.Default.ReadUserDto);
        }
    }
}
