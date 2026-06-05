using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Business;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.Localization;
using com.etsoo.ServiceApp.SmartERP;
using com.etsoo.Utils.Actions;
using CRM.Server.Dto.Dept;
using CRM.Server.RQ.Dept;
using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Org;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using PlatformShared.Extentions;
using System.Buffers;
using System.Text.Json;

namespace CRM.Server.Services
{
    /// <summary>
    /// Department service
    /// 部门服务
    /// </summary>
    public class DeptService : SEUserService, IDeptService
    {
        readonly MyDbContext _db;
        readonly ICommonService _commonService;
        readonly IQueueService _queueService;

        public DeptService(
            MyDbContext db,
            ISEServiceApp app,
            CurrentUserAccessor userAccessor,
            ILogger<DeptService> logger,
            ICommonService commonService,
            IQueueService queueService
        )
            : base(app, userAccessor.UserSafe, "dept", logger)
        {
            _db = db;
            _commonService = commonService;
            _queueService = queueService;
        }

        /// <summary>
        /// Create department
        /// 创建部门
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> CreateAsync(DeptCreateRQ rq, CancellationToken cancellationToken = default)
        {
            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.Dept.Add, cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            // Organization id
            var orgId = User.OrganizationInt;

            // No same name
            var sameTitle = await _db.Depts(orgId).AsNoTracking()
                .Where(p => p.Name == rq.Name)
                .AnyAsync(cancellationToken);

            if (sameTitle)
            {
                return ApplicationErrors.ItemExists.AsResult(nameof(rq.Name));
            }

            var keyword = ChineseUtils.GetPinyin(rq.Name, false).ToInitials();

            var dept = new Person
            {
                OrgId = orgId,
                IdentityType = IdentityTypeFlags.Dept,
                IsLegalPerson = true,
                Name = rq.Name,
                QueryKeyword = keyword,
                UserId = rq.LeaderId ?? User.Oid,
                Status = rq.Status ?? EntityStatus.Normal
            };

            // Add
            _db.Persons.Add(dept);

            // Save
            await _db.SaveChangesAsync(cancellationToken);

            var id = dept.Id;

            // Push message
            var message = new CreateDeptMessage
            {
                Data = User.CreateMessageData(App.AppId, id, dept.Name),
                JsonData = JsonSerializer.Serialize(rq, MyJsonSerializerContext.Default.DeptCreateRQ)
            };
            await _queueService.PushAsync(message, CrmJsonSerializerContext.Default.CreateDeptMessage, cancellationToken);

            // Return
            return ActionResult.Succeed(id);
        }

        private IQueryable<Person> CreateQuery(DeptListRQ rq, Func<IQueryable<Person>, IQueryable<Person>>? filters = null)
        {
            var query = _db.Depts(User.OrganizationInt).AsNoTracking()
                .QueryEtsoo(rq, (d) => d.Id, (d) => d.Status, (q) =>
                {
                    if (rq.LeaderId.HasValue)
                    {
                        q = q.Where(d => d.UserId == rq.LeaderId);
                    }

                    if (rq.Keyword?.Length > 1)
                    {
                        var keyword = rq.Keyword;

                        if (keyword.IsComplexQueryKeywords())
                        {
                            q = q.QueryEtsooKeywords(keyword, DbUtils.ILikeMethod, d => d.Name, d => d.Description);
                        }
                        else
                        {
                            q = q.Where(d => EF.Functions.ILike(d.Name, $"%{keyword}%")
                            || (d.QueryKeyword != null && EF.Functions.ILike(d.QueryKeyword, $"%{keyword}%"))
                            || (d.Description != null && EF.Functions.ILike(d.Description, $"%{keyword}%"))
                            );
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
        /// List department JSON data
        /// 部门列表JSON数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task ListAsync(DeptListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.Dept.List, cancellationToken))
            {
                return;
            }

            await CreateQuery(rq)
                .Select(d => new DeptListData
                {
                    Id = d.Id,
                    Name = d.Name
                }).ToJsonAsync(writer, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Query department JSON data
        /// 查询部门JSON数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task QueryAsync(DeptQueryRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.Dept.Query, cancellationToken))
            {
                return;
            }

            await CreateQuery(rq)
                .Select(d => new DeptQueryData
                {
                    Id = d.Id,
                    Name = d.Name,
                    Leader = d.User.Name,
                    Staff = d.Contacts.Where(c => c.Person.Status <= EntityStatus.Approved).Count(),
                    Status = d.Status,
                    Creation = d.Creation
                }).ToJsonAsync(writer, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Update department
        /// 更新部门
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> UpdateAsync(DeptUpdateRQ rq, CancellationToken cancellationToken = default)
        {
            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.Dept.Edit, cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            // Organization id
            var orgId = User.OrganizationInt;

            // Validate the leader id
            if (rq.LeaderId.HasValue && !await _db.Users(orgId).Where(u => u.Id == rq.LeaderId).AnyAsync(cancellationToken))
            {
                return ApplicationErrors.NoId.AsResult(nameof(rq.LeaderId));
            }

            var dept = await _db.Depts(orgId).FirstOrDefaultAsync(cancellationToken);
            if (dept == null)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            if (rq.IsModified(nameof(rq.Name)) && !string.IsNullOrEmpty(rq.Name))
            {
                dept.Name = rq.Name;

                var keyword = ChineseUtils.GetPinyin(rq.Name, false).ToInitials();
                dept.QueryKeyword = keyword;
            }

            if (rq.IsModified(nameof(rq.LeaderId)) && rq.LeaderId.HasValue)
            {
                dept.UserId = rq.LeaderId.Value;
            }

            if (rq.IsModified(nameof(rq.Status)) && rq.Status.HasValue)
            {
                dept.Status = rq.Status.Value;
            }

            // Changes
            var changes = _db.ChangeTracker.Entries().GetChangedProperties();

            // Save
            var task1 = _db.SaveChangesAsync(cancellationToken);

            // Push message
            var message = new UpdateDeptMessage
            {
                Data = User.CreateMessageData(App.AppId, rq.Id, dept.Name),
                Changes = changes
            };
            var task2 = _queueService.PushAsync(message, CrmJsonSerializerContext.Default.UpdateDeptMessage, cancellationToken);

            await Task.WhenAll(task1, task2);

            // Return
            return ActionResult.Succeed(rq.Id);
        }

        /// <summary>
        /// Read department data for update
        /// 读取用于更新的部门数据
        /// </summary>
        /// <param name="id">Dept id</param>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task UpdateReadAsync(long id, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.Dept.Edit, cancellationToken))
            {
                return;
            }

            await _db.Depts(User.OrganizationInt).AsNoTracking()
                .Where(d => d.Id == id)
                .Select(d => new
                {
                    d.Id,
                    d.Name,
                    LeaderId = d.UserId,
                    d.Status
                }).ToJsonObjectAsync(writer, cancellationToken: cancellationToken);
        }
    }
}