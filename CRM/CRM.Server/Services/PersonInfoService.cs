using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.ServiceApp.SmartERP;
using com.etsoo.Utils.Actions;
using CRM.Server.RQ.PersonInfo;
using Microsoft.EntityFrameworkCore;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using System.Buffers;

namespace CRM.Server.Services
{
    /// <summary>
    /// Person info service
    /// 人员信息服务
    /// </summary>
    public class PersonInfoService : SEUserService, IPersonInfoService
    {
        readonly MyDbContext _db;
        readonly ICommonService _commonService;

        public PersonInfoService(
            MyDbContext db,
            ISEServiceApp app,
            CurrentUserAccessor userAccessor,
            ILogger<PersonInfoService> logger,
            ICommonService commonService
        )
            : base(app, userAccessor.UserSafe, "person_info", logger)
        {
            _db = db;
            _commonService = commonService;
        }

        /// <summary>
        /// Create
        /// 创建
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> CreateAsync(PersonInfoCreateRQ rq, CancellationToken cancellationToken = default)
        {
            // Organization id
            var orgId = User.OrganizationInt;

            var person = await _db.Persons
               .Where(p => p.Id == rq.PersonId && p.OrgId == orgId)
               .Select(p => new Person
               {
                   Id = p.Id,
                   IdentityType = p.IdentityType,
                   Infos = p.Infos
               })
               .FirstOrDefaultAsync(cancellationToken);

            if (person == null)
            {
                return ApplicationErrors.NoId.AsResult(nameof(rq.PersonId));
            }

            if (!await _commonService.HasIdentityPermissionAsync(person.IdentityType, nameof(Permissions.Customer.Edit), cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            _db.Persons.Attach(person);

            // Create infos
            foreach (var item in rq.Items)
            {
                // Format identifier
                var identifier = item.Identifier.Trim().ToLower();

                // Check if the info already exists
                if (person.Infos.Any(i => i.Kind == item.Kind && i.Identifier == identifier))
                {
                    continue;
                }

                // Create new info
                var info = new PersonInfo
                {
                    PersonId = person.Id,
                    Kind = item.Kind,
                    Identifier = identifier,
                    Description = item.Description,
                    IsDefault = item.IsDefault ?? false
                };

                person.Infos.Add(info);
            }

            // Save changes
            var affected = await _db.SaveChangesAsync(cancellationToken);

            if (affected == 0)
            {
                return ApplicationErrors.ItemExists.AsResult();
            }

            // Return
            return ActionResult.Succeed(rq.PersonId);
        }

        /// <summary>
        /// Delete info
        /// 删除信息
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            // Organization id
            var orgId = User.OrganizationInt;

            var info = await _db.PersonInfos
               .Where(i => i.Id == id && i.Person.OrgId == orgId)
               .Select(i => new { i.Person.IdentityType })
               .FirstOrDefaultAsync(cancellationToken);

            if (info == null)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            if (!await _commonService.HasIdentityPermissionAsync(info.IdentityType, nameof(Permissions.Customer.Edit), cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            var result = await _db.PersonInfos.AsNoTracking()
                .Where(p => p.Id == id)
                .ExecuteDeleteAsync(cancellationToken);

            if (result == 0)
            {
                return ApplicationErrors.NoId.AsResult();
            }
            else
            {
                return ActionResult.Succeed(id);
            }
        }

        /// <summary>
        /// Query person info JSON data
        /// 查询人员信息JSON数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public Task QueryAsync(PersonInfoQueryRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            var orgId = User.OrganizationInt;

            return _db.PersonInfos.Where(i => i.PersonId == rq.PersonId && i.Person.OrgId == orgId)
                .AsNoTracking()
                .QueryEtsoo(rq, (i) => i.Id, null, (q) =>
                {
                    if (rq.Identifier?.Length > 1)
                    {
                        var identifier = rq.Identifier.Trim().ToLower();
                        q = q.Where(i => i.Identifier == identifier);
                    }

                    if (rq.Kind.HasValue)
                    {
                        q = q.Where(i => i.Kind == rq.Kind.Value);
                    }

                    if (rq.IsDefault.HasValue)
                    {
                        q = q.Where(i => i.IsDefault == rq.IsDefault);
                    }

                    if (rq.IsVerified.HasValue)
                    {
                        q = q.Where(i => i.IsVerified == rq.IsVerified);
                    }

                    if (rq.Subscribed.HasValue)
                    {
                        q = q.Where(i => i.Subscribed == rq.Subscribed);
                    }

                    if (rq.Keyword?.Length > 1)
                    {
                        q = q.QueryEtsooKeywords(rq.Keyword, DbUtils.ILikeMethod, p => p.Description);
                    }

                    return q;
                })
                .Select(i => new
                {
                    i.Id,
                    i.Kind,
                    Identifier = MyDbFunctions.HideData(i.Identifier, '@'),
                    i.Description,
                    i.IsDefault,
                    i.IsVerified,
                    i.Subscribed,
                    i.Creation
                })
                .ToJsonAsync(writer, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Read person info for view
        /// 读取用于浏览的人员信息
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<string?> ReadAsync(int id, CancellationToken cancellationToken = default)
        {
            // Organization id
            var orgId = User.OrganizationInt;

            var info = await _db.PersonInfos
               .Where(i => i.Id == id && i.Person.OrgId == orgId)
               .Select(i => new { i.Identifier, i.Person.IdentityType })
               .FirstOrDefaultAsync(cancellationToken);

            if (info == null)
            {
                return null;
            }

            if (!await _commonService.HasIdentityPermissionAsync(info.IdentityType, nameof(Permissions.Customer.View), cancellationToken))
            {
                return null;
            }

            return info.Identifier;
        }

        /// <summary>
        /// Update
        /// 更新
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> UpdateAsync(PersonInfoUpdateRQ rq, CancellationToken cancellationToken = default)
        {
            // Organization id
            var orgId = User.OrganizationInt;

            var info = await _db.PersonInfos
                .Where(i => i.Id == rq.Id && i.Person.OrgId == orgId)
                .Include(i => i.Person)
                .Select(i => new PersonInfo
                {
                    Id = i.Id,
                    Kind = i.Kind,
                    Identifier = i.Identifier,
                    Description = i.Description,
                    IsDefault = i.IsDefault,
                    Subscribed = i.Subscribed,
                    Person = new Person
                    {
                        Id = i.Person.Id,
                        IdentityType = i.Person.IdentityType
                    }
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (info == null)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            if (!await _commonService.HasIdentityPermissionAsync(info.Person.IdentityType, nameof(Permissions.Customer.Edit), cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            _db.PersonInfos.Attach(info);

            if (rq.IsModified(nameof(rq.Kind)))
            {
                info.Kind = rq.Kind;
            }

            if (rq.IsModified(nameof(rq.Identifier)) && !string.IsNullOrEmpty(rq.Identifier))
            {
                info.Identifier = rq.Identifier;
            }

            if (rq.IsModified(nameof(rq.Description)))
            {
                info.Description = rq.Description;
            }

            if (rq.IsModified(nameof(rq.IsDefault)) && rq.IsDefault.HasValue)
            {
                info.IsDefault = rq.IsDefault.Value;
            }

            if (rq.IsModified(nameof(rq.Subscribed)))
            {
                info.Subscribed = rq.Subscribed;
            }

            // Save
            await _db.SaveChangesAsync(cancellationToken);

            // Return
            return ActionResult.Succeed(rq.Id);
        }
    }
}
