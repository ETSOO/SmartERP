using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Authentication;
using com.etsoo.CoreFramework.Business;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.HTTP;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Storage;
using Microsoft.EntityFrameworkCore;
using Platform.Server.Application;
using Platform.Server.Dto.Org;
using Platform.Server.Endpoints.Org.RQ;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using System.Buffers;

namespace Platform.Server.Services
{
    /// <summary>
    /// Organization service
    /// 机构服务
    /// </summary>
    public class OrgService : CommonUserService, IOrgService
    {
        readonly MyDbContext _db;
        readonly IPublicService _publicService;
        readonly IStorage _storage;

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="db">Database EF</param>
        /// <param name="app">Application</param>
        /// <param name="userAccessor">User accessor</param>
        /// <param name="logger">Logger</param>
        /// <param name="publicService">Public service</param>
        /// <param name="storage">Storage</param>
        public OrgService(MyDbContext db, IMyApp app, CurrentUserAccessor userAccessor, ILogger<UserService> logger, IPublicService publicService, IStorage storage)
            : base(app, userAccessor.UserSafe, "org", logger)
        {
            _db = db;
            _publicService=publicService;
            _storage=storage;
        }

        /// <summary>
        /// Create organization
        /// 创建机构
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> CreateAsync(OrgCreateRQ rq, CancellationToken cancellationToken = default)
        {
            var (result, id) = await CreateWithIdAsync(rq, cancellationToken);
            if (id.HasValue) return ActionResult.Succeed(id.Value);
            return result;
        }

        /// <summary>
        /// Create organization with returning id
        /// 创建机构并返回编号
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<(IActionResult result, int? id)> CreateWithIdAsync(OrgCreateRQ rq, CancellationToken cancellationToken = default)
        {
            // Check exists
            var exists = await _db.CoreOrganizations.AnyAsync(o => o.Name == rq.Name && o.Pin == rq.Pin, cancellationToken);
            if (exists)
            {
                return (ApplicationErrors.OrgExists.AsResult("Name"), null);
            }

            // Default QueryKeyword
            var queryKeyword = string.IsNullOrEmpty(rq.QueryKeyword)
                ? _publicService.GetPinyin(new PinyinRQ { Input = rq.Name, Format = PinyinFormatType.Initial })
                : rq.QueryKeyword
            ;

            // Create organization
            var org = new CoreOrganization
            {
                OwnerId = User.IdInt,
                Name = rq.Name,
                Brand = rq.Brand,
                Pin = rq.Pin,
                ParentId = rq.ParentId,
                Status = rq.Status.GetValueOrDefault(),
                QueryKeyword = queryKeyword,
                CoreOrganizationUsers = [
                    new CoreOrganizationUser
                    {
                        CoreUserId = User.IdInt,
                        UserRole = UserRole.Founder
                    }
                ]
            };

            // Add
            _db.CoreOrganizations.Add(org);

            // Save
            await _db.SaveChangesAsync(cancellationToken);

            // Return
            return (ActionResult.Success, org.Id);
        }

        /// <summary>
        /// Delete organization
        /// 删除机构
        /// </summary>
        /// <param name="id">Organization id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var orgExists = await _db.CoreOrganizations.AsNoTracking().AnyAsync(o => o.Id == id && o.OwnerId == User.IdInt, cancellationToken);
            if (!orgExists)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            var userExists = await _db.CoreOrganizationUsers.AsNoTracking().AnyAsync(ou => ou.CoreOrganizationId == id && ou.CoreUserId != User.IdInt, cancellationToken);
            if (userExists)
            {
                return ApplicationErrors.DeleteReferencedData.AsResult("User");
            }

            var appExists = await _db.CoreOrganizationApps.AsNoTracking().AnyAsync(oa => oa.CoreOrganizationId == id, cancellationToken);
            if (appExists)
            {
                return ApplicationErrors.DeleteReferencedData.AsResult("App");
            }

            await _db.CoreOrganizationUsers.Where(ou => ou.CoreOrganizationId == id && ou.CoreUserId == User.IdInt).ExecuteDeleteAsync(cancellationToken);
            await _db.CoreOrganizations.Where(o => o.Id == id).ExecuteDeleteAsync(cancellationToken);

            return ActionResult.Succeed(id);
        }

        /// <summary>
        /// Get user's latest accessed organizations
        /// 获取用户最近访问的机构
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public async Task GetMyAsync(OrgGetMyRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            var (hasContent, commandText) = await _db.CoreOrganizationUsers
                .AsNoTracking()
                .Where(ou => ou.CoreOrganizationId == User.OrganizationInt
                    && ou.Status <= EntityStatus.Approved
                    && (ou.Expiry == null || ou.Expiry >= DateTimeOffset.Now)
                    && ou.CoreOrganization.Status <= EntityStatus.Approved)
                .OrderByDescending(ou => ou.Id)
                .Take(rq.MaxItems)
                .Select(ou => new OrgGetMyData
                {
                    Id = ou.CoreOrganizationId,
                    Name = ou.CoreOrganization.Name,
                    Brand = ou.CoreOrganization.Brand
                }).ToJsonAsync(writer, cancellationToken: cancellationToken);

            if (_db.IsSensitiveDataLoggingEnabled)
            {
                Logger.LogInformation("GetMyAsync is {hasContent} with {commandText}", hasContent, commandText);
            }
        }

        /// <summary>
        /// List organization JSON data
        /// 机构列表JSON数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task ListAsync(OrgListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            var query = CreateQuery(rq);

            await query.Select(o => new OrgListData
            {
                Id = o.Id,
                Name = o.Name,
                Pin = MyDbFunctions.HideData(o.Pin, default)
            }).ToJsonAsync(writer, cancellationToken: cancellationToken);
        }

        private IQueryable<CoreOrganization> CreateQuery(OrgListRQ rq, Func<IQueryable<CoreOrganization>, IQueryable<CoreOrganization>>? filters = null)
        {
            var query = _db.CoreOrganizations
                .AsNoTracking()
                .Where(o => o.CoreOrganizationUsers.Any(ou => ou.CoreUserId == User.IdInt))
                .QueryEtsoo(rq, (o) => o.Id, (o) => o.Status, (q) =>
                {
                    if (rq.ParentId.HasValue)
                    {
                        q = q.Where(o => o.ParentId == rq.ParentId);
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
                            q = q.Where(o => o.Brand == keyword || EF.Functions.ILike(o.Name, $"%{keyword}%") || (o.Pin != null && EF.Functions.ILike(o.Pin, $"%{keyword}%")) || (o.QueryKeyword != null && EF.Functions.ILike(o.QueryKeyword, $"%{keyword}%")));
                        }
                    }

                    if (filters != null)
                    {
                        q = filters(q);
                    }

                    return q;
                });

            return query;
        }

        /// <summary>
        /// Check if the user owns the organization
        /// 检查用户是否拥有机构
        /// </summary>
        /// <param name="id">Org id</param>
        /// <param name="userRole">Minimum user role</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<bool> OwnsAsync(int id, UserRole userRole = UserRole.Guest, CancellationToken cancellationToken = default)
        {
            return await _db.CoreOrganizationUsers.AsNoTracking()
                .AnyAsync(ou => ou.CoreOrganizationId == id
                    && ou.CoreUserId == User.IdInt
                    && ou.Status <= EntityStatus.Approved
                    && ou.UserRole >= userRole
                    && (ou.Expiry == null || ou.Expiry >= DateTimeOffset.UtcNow), cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Query organization
        /// 查询机构
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IEnumerable<OrgQueryData>> QueryAsync(OrgQueryRQ rq, CancellationToken cancellationToken = default)
        {
            var query = CreateQuery(rq, (q) =>
            {
                if (rq.Pin?.Length > 1)
                {
                    q = q.Where(o => o.Pin != null && EF.Functions.ILike(o.Pin, $"%{rq.Pin}%"));
                }

                return q;
            });

            var data = await query.Select(o => new OrgQueryData
            {
                Id = o.Id,
                Name = o.Name,
                IsOwner = o.OwnerId == User.IdInt,
                Brand = o.Brand,
                Pin = o.Pin,
                ParentId = o.ParentId,
                Status = o.Status
            }).ToListAsync(cancellationToken);

            return data;
        }

        /// <summary>
        /// Query organization JSON data
        /// 查询机构JSON数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task QueryAsync(OrgQueryRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            var query = CreateQuery(rq, (q) =>
            {
                if (rq.Pin?.Length > 1)
                {
                    q = q.Where(o => o.Pin != null && EF.Functions.ILike(o.Pin, $"%{rq.Pin}%"));
                }

                return q;
            });

            var (hasContent, commandText) = await query.Select(o => new OrgQueryData
            {
                Id = o.Id,
                Name = o.Name,
                IsOwner = o.OwnerId == User.IdInt,
                Brand = o.Brand,
                Pin = MyDbFunctions.HideData(o.Pin, default),
                ParentId = o.ParentId,
                Status = o.Status,
                Creation = o.Creation
            }).ToJsonAsync(writer, cancellationToken: cancellationToken);

            if (_db.IsSensitiveDataLoggingEnabled)
            {
                Logger.LogInformation("QueryAsync is {hasContent} with {commandText}", hasContent, commandText);
            }
        }

        /// <summary>
        /// Read organization data for view
        /// 读取用于浏览的机构数据
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="response">Http response to write</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task ReadAsync(int id, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            var query = _db.CoreOrganizations
                .AsNoTracking()
                .Where(o => o.Id == id && o.CoreOrganizationUsers.Any(ou => ou.CoreUserId == User.IdInt));

            var (hasContent, _) = await query.Select(o => new
            {
                o.Id,
                OwnerName = o.Owner.Name,
                o.Name,
                o.Brand,
                o.Logo,
                o.Pin,
                ParentName = (o.Parent == null ? null : o.Parent.Name),
                o.ParentId,
                o.Uid,
                o.Creation,
                o.Status,
                o.QueryKeyword
            }).ToJsonObjectAsync(writer, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Update organization
        /// 更新机构
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> UpdateAsync(OrgUpdateRQ rq, CancellationToken cancellationToken = default)
        {
            var org = await _db.CoreOrganizations.FirstOrDefaultAsync(o => o.Id == rq.Id && o.OwnerId == User.IdInt, cancellationToken);
            if (org == null)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            // QueryKeyword
            var isQueryKeywordModified = rq.IsModified(nameof(rq.QueryKeyword));

            // Update organization
            if (rq.IsModified(nameof(rq.Name)) && !string.IsNullOrEmpty(rq.Name))
            {
                org.Name = rq.Name;
                if (!isQueryKeywordModified)
                {
                    org.QueryKeyword = _publicService.GetPinyin(new PinyinRQ { Input = rq.Name, Format = PinyinFormatType.Initial });
                }
            }

            if (rq.IsModified(nameof(rq.Brand)))
            {
                org.Brand = rq.Brand;
            }

            if (rq.IsModified(nameof(rq.Pin)))
            {
                org.Pin = rq.Pin;
            }

            if (rq.IsModified(nameof(rq.ParentId)))
            {
                org.ParentId = rq.ParentId;
            }

            if (rq.IsModified(nameof(rq.Status)) && rq.Status.HasValue)
            {
                org.Status = rq.Status.Value;
            }

            if (isQueryKeywordModified)
            {
                org.QueryKeyword = rq.QueryKeyword;
            }

            // Save
            await _db.SaveChangesAsync(cancellationToken);

            // Return
            return ActionResult.Succeed(rq.Id);
        }

        /// <summary>
        /// Update avatar
        /// 更新头像
        /// </summary>
        /// <param name="id">Organization id</param>
        /// <param name="avatarStream">Avatar stream</param>
        /// <param name="contentType">Cotent type</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>New URL</returns>
        public async Task<IActionResult> UpdateAvatarAsync(int id, Stream avatarStream, string contentType, CancellationToken cancellationToken = default)
        {
            // Check the stream
            if (avatarStream.Length is not > 10240 and < 102400000)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(avatarStream));
            }

            // Check the organization id
            var org = await _db.CoreOrganizations.AsNoTracking()
                .Where(o => o.Id == id && o.OwnerId == User.IdInt)
                .Select(o => new { o.Logo })
                .FirstOrDefaultAsync(cancellationToken);
            if (org == null)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            var extension = MimeTypeMap.TryGetExtension(contentType);
            if (string.IsNullOrEmpty(extension))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(contentType));
            }

            // File path
            var path = "/OrgAvatar/" + DateTime.UtcNow.ToString("yyyyMM") + "/" + Path.GetRandomFileName() + extension;

            // Save the stream to file directly
            var saveResult = await _storage.WriteAsync(path, avatarStream, WriteCase.CreateNew, cancellationToken: cancellationToken);

            if (saveResult)
            {
                // New avatar URL
                var url = _storage.GetUrl(path);

                // Update
                await _db.CoreOrganizations.Where(o => o.Id == id).ExecuteUpdateAsync(o => o.SetProperty(o => o.Logo, url), cancellationToken);

                // Remove current avatar
                if (!string.IsNullOrEmpty(org.Logo))
                    await _storage.DeleteUrlAsync(org.Logo, cancellationToken);

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
        /// Read organization data for update
        /// 读取用于更新的机构数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task UpdateReadAsync(int id, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            var query = _db.CoreOrganizations
                .AsNoTracking()
                .Where(o => o.Id == id && o.OwnerId == User.IdInt);

            await query.Select(o => new
            {
                o.Id,
                o.Name,
                o.Brand,
                o.Logo,
                o.Pin,
                o.ParentId,
                o.Status,
                o.QueryKeyword
            }).ToJsonObjectAsync(writer, cancellationToken: cancellationToken);
        }
    }
}
