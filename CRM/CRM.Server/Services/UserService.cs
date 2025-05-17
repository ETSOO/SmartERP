using com.etsoo.CoreFramework.Business;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.ServiceApp.SmartERP;
using CRM.Server.Dto.User;
using CRM.Server.RQ.User;
using Microsoft.EntityFrameworkCore;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using PlatformShared.Extentions;
using System.Buffers;

namespace CRM.Server.Services
{
    /// <summary>
    /// User service
    /// 用户服务
    /// </summary>
    public class UserService : SEUserService, IUserService
    {
        readonly MyDbContext _db;

        public UserService(
            MyDbContext db,
            ISEServiceApp app,
            CurrentUserAccessor userAccessor,
            ILogger<UserService> logger
        )
            : base(app, userAccessor.UserSafe, "user", logger)
        {
            _db = db;
        }

        private IQueryable<Person> CreateQuery(UserListRQ rq, Func<IQueryable<Person>, IQueryable<Person>>? filters = null)
        {
            var query = _db.Users(User.OrganizationInt).AsNoTracking()
                .QueryEtsoo(rq, (ou) => ou.Id, (ou) => ou.Status, (q) =>
                {
                    if (rq.GroupId.HasValue)
                    {
                        q = q.Where(ou => ou.PermissionGroups != null && ou.PermissionGroups.Contains(rq.GroupId.Value));
                    }

                    if (rq.DeptId.HasValue)
                    {
                        q = q.Where(ou => ou.ContactOwners != null && ou.ContactOwners.Any(o => o.PersonId == rq.DeptId && (o.Person.IdentityType & IdentityTypeFlags.Dept) > 0));
                    }

                    if (rq.Keyword?.Length > 1)
                    {
                        var keyword = rq.Keyword;

                        if (keyword.IsComplexQueryKeywords())
                        {
                            q = q.QueryEtsooKeywords(keyword, DbUtils.ILikeMethod, ou => ou.Name, ou => ou.PreferredName);
                        }
                        else
                        {
                            q = q.Where(ou => EF.Functions.ILike(ou.Name, $"%{keyword}%")
                            || (ou.QueryKeyword != null && EF.Functions.ILike(ou.QueryKeyword, $"%{keyword}%"))
                            || (ou.PreferredName != null && EF.Functions.ILike(ou.PreferredName, $"%{keyword}%"))
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
        /// List person JSON data
        /// 人员列表JSON数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task ListAsync(UserListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            await CreateQuery(rq)
                .Select(ou => new UserListData
                {
                    Id = ou.Id,
                    Name =ou.Name
                }).ToJsonAsync(writer, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Query person JSON data
        /// 查询人员JSON数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public Task<UserQueryData[]> QueryAsync(UserQueryRQ rq, CancellationToken cancellationToken = default)
        {
            return CreateQuery(rq)
                .Select(u => new UserQueryData
                {
                    Id = u.Id,
                    Name = u.Name,
                    UserRole = u.UserRole,
                    Depts = u.ContactOwners
                        .Where(o => (o.Person.IdentityType & IdentityTypeFlags.Dept) > 0)
                        .Select(o => o.Person.Name),
                    Status = u.Status,
                    Creation = u.Creation
                }).ToArrayAsync(cancellationToken);
        }
    }
}