using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.ServiceApp.SmartERP;
using CRM.Server.Dto.Group;
using CRM.Server.RQ.Group;
using Microsoft.EntityFrameworkCore;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using System.Buffers;

namespace CRM.Server.Services
{
    /// <summary>
    /// Permission group service
    /// 权限组服务
    /// </summary>
    public class GroupService : SEUserService, IGroupService
    {
        readonly MyDbContext _db;
        readonly ICommonService _commonService;

        public GroupService(
            MyDbContext db,
            ISEServiceApp app,
            CurrentUserAccessor userAccessor,
            ILogger<GroupService> logger,
            ICommonService commonService
        )
            : base(app, userAccessor.UserSafe, "group", logger)
        {
            _db = db;
            _commonService = commonService;
        }

        private IQueryable<PermissionGroup> CreateQuery(GroupListRQ rq, Func<IQueryable<PermissionGroup>, IQueryable<PermissionGroup>>? filters = null)
        {
            var query = _db.PermissionGroups.AsNoTracking()
                .Where(g => (g.CoreOrganizationId == null || g.CoreOrganizationId == User.OrganizationInt))
                .QueryEtsoo(rq, (g) => g.Id, null, (q) =>
                {
                    if (rq.Keyword?.Length > 1)
                    {
                        var keyword = rq.Keyword;

                        if (keyword.IsComplexQueryKeywords())
                        {
                            q = q.QueryEtsooKeywords(keyword, DbUtils.ILikeMethod, g => g.Name);
                        }
                        else
                        {
                            q = q.Where(d => EF.Functions.ILike(d.Name, $"%{keyword}%"));
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
        /// List permission group JSON data
        /// 权限组列表JSON数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task ListAsync(GroupListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            await CreateQuery(rq)
                .Select(d => new GroupListData
                {
                    Id = d.Id,
                    Name = d.Name
                }).ToJsonAsync(writer, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Query permission group JSON data
        /// 查询权限组JSON数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task QueryAsync(GroupQueryRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            await CreateQuery(rq)
                .Select(d => new GroupQueryData
                {
                    Id = d.Id,
                    Name = d.Name
                }).ToJsonAsync(writer, cancellationToken: cancellationToken);
        }
    }
}