using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Authentication;
using com.etsoo.CoreFramework.Business;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.Utils.Actions;
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

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="db">Database EF</param>
        /// <param name="app">Application</param>
        /// <param name="userAccessor">User accessor</param>
        /// <param name="logger">Logger</param>
        /// <param name="publicService">Public service</param>
        public OrgService(MyDbContext db, IMyApp app, CurrentUserAccessor userAccessor, ILogger<PublicService> logger, IPublicService publicService)
            : base(app, userAccessor.UserSafe, "org", logger)
        {
            _db = db;
            _publicService=publicService;
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
            // Check exists
            var exists = await _db.CoreOrganizations.AnyAsync(o => o.Name == rq.Name && o.Pin == rq.Pin, cancellationToken);
            if (exists)
            {
                return ApplicationErrors.OrgExists.AsResult("Name");
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
            var result = ActionResult.Success;
            result.Data["Id"] = org.Id;
            return result;
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

            return ActionResult.Success;
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
            var query = _db.CoreOrganizations
                .AsNoTracking()
                .Where(o => o.CoreOrganizationUsers.Any(ou => ou.CoreUserId == User.IdInt));

            var keyword = rq.Keyword;
            if (!string.IsNullOrEmpty(keyword) && keyword.Length > 1)
            {
                query = query.Where(o => o.Brand == keyword || o.Name.Contains(keyword) || (o.QueryKeyword != null && o.QueryKeyword.Contains(keyword)));
            }

            if (!string.IsNullOrEmpty(rq.Pin))
            {
                query = query.Where(o => o.Pin != null && o.Pin.Contains(rq.Pin));
            }

            var data = await query.QueryEtsoo(rq, (o) => o.Id, (o) => o.Status)
                .Select(o => new OrgQueryData
                {
                    Id = o.Id,
                    Name = o.Name,
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
        public async Task QueryJsonAsync(OrgQueryRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            var query = _db.CoreOrganizations
                .AsNoTracking()
                .Where(o => o.CoreOrganizationUsers.Any(ou => ou.CoreUserId == User.IdInt));

            var keyword = rq.Keyword;
            if (!string.IsNullOrEmpty(keyword) && keyword.Length > 1)
            {
                query = query.Where(o => o.Brand == keyword || o.Name.Contains(keyword) || (o.QueryKeyword != null && o.QueryKeyword.Contains(keyword)));
            }

            if (!string.IsNullOrEmpty(rq.Pin))
            {
                query = query.Where(o => o.Pin != null && o.Pin.Contains(rq.Pin));
            }

            await query.QueryEtsoo(rq, (o) => o.Id, (o) => o.Status)
                .Select(o => new OrgQueryData
                {
                    Id = o.Id,
                    Name = o.Name,
                    Brand = o.Brand,
                    Pin = o.Pin,
                    ParentId = o.ParentId,
                    Status = o.Status
                }).ToJsonAsync(writer, cancellationToken: cancellationToken);
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
            return ActionResult.Success;
        }
    }
}
