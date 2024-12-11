using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.Utils.Actions;
using Microsoft.EntityFrameworkCore;
using Platform.Server.Application;
using Platform.Server.Dto.Member;
using Platform.Server.Endpoints.Member.RQ;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using System.Buffers;

namespace Platform.Server.Services
{
    /// <summary>
    /// Member service
    /// 成员服务
    /// </summary>
    public class MemberService : CommonUserService, IMemberService
    {
        readonly MyDbContext _db;

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="db">Database EF</param>
        /// <param name="app">Application</param>
        /// <param name="userAccessor">User accessor</param>
        /// <param name="logger">Logger</param>
        public MemberService(MyDbContext db, IMyApp app, CurrentUserAccessor userAccessor, ILogger<UserService> logger)
            : base(app, userAccessor.UserSafe, "member", logger)
        {
            _db = db;
        }

        /// <summary>
        /// Delete member
        /// 删除成员
        /// </summary>
        /// <param name="id">Organization id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var result = await _db.CoreOrganizationUsers.Where(ou => ou.Id == id && ou.CoreOrganizationId == User.OrganizationInt && ou.CoreOrganization.OwnerId != User.IdInt).ExecuteDeleteAsync(cancellationToken);

            if (result < 1)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            return ActionResult.Succeed(id);
        }

        private IQueryable<CoreOrganizationUser> CreateQuery(MemberListRQ rq, Func<IQueryable<CoreOrganizationUser>, IQueryable<CoreOrganizationUser>>? filters = null)
        {
            var query = _db.CoreOrganizationUsers
                .AsNoTracking()
                .Where(ou => ou.CoreOrganizationId == User.OrganizationInt)
                .QueryEtsoo(rq, (ou) => ou.Id, (ou) => ou.Status, (q) =>
                {
                    if (rq.ExcludeSelf is true)
                    {
                        q = q.Where(ou => ou.CoreUserId != User.IdInt);
                    }

                    if (rq.UserRole.HasValue)
                    {
                        q = q.Where(ou => ou.UserRole == rq.UserRole);
                    }

                    if (rq.UserRoleStart.HasValue)
                    {
                        q = q.Where(ou => ou.UserRole >= rq.UserRoleStart);
                    }

                    if (rq.Keyword?.Length > 1)
                    {
                        var keyword = rq.Keyword;

                        if (keyword.IsComplexQueryKeywords())
                        {
                            q = q.QueryEtsooKeywords(keyword, DbUtils.ILikeMethod, ou => ou.LocalName ?? ou.CoreUser.Name);
                        }
                        else
                        {
                            q = q.Where(ou => EF.Functions.ILike(ou.LocalName ?? ou.CoreUser.Name, $"%{keyword}%") ||(ou.AssignedId != null && EF.Functions.ILike(ou.AssignedId, $"%{keyword}%")));
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
        /// List member JSON data
        /// 成员列表JSON数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task ListAsync(MemberListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            var query = CreateQuery(rq);

            await query.Select(ou => new
            {
                ou.Id,
                Name = ou.LocalName ?? ou.CoreUser.Name
            }).ToJsonAsync(writer, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Query member JSON data
        /// 查询成员JSON数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task QueryAsync(MemberQueryRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            var query = CreateQuery(rq, (q) =>
            {
                if (rq.AssignedId?.Length > 1)
                {
                    q = q.Where(ou => ou.AssignedId != null && EF.Functions.ILike(ou.AssignedId, $"%{rq.AssignedId}%"));
                }

                return q;
            });

            var (hasContent, commandText) = await query.Select(ou => new MemberQueryData
            {
                Id = ou.Id,
                Name = ou.LocalName ?? ou.CoreUser.Name,
                UserRole = ou.UserRole,
                AssignedId = ou.AssignedId,
                IsOwner = ou.CoreOrganization.OwnerId == User.IdInt,
                IsSelf = ou.CoreUserId == User.IdInt,
                Status = ou.Status,
                Creation = ou.Creation
            }).ToJsonAsync(writer, cancellationToken: cancellationToken);

            if (_db.IsSensitiveDataLoggingEnabled)
            {
                Logger.LogInformation("QueryAsync is {hasContent} with {commandText}", hasContent, commandText);
            }
        }
    }
}
