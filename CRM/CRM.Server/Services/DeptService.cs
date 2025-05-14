using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.ServiceApp.SmartERP;
using CRM.Server.Dto.Dept;
using CRM.Server.RQ.Dept;
using Microsoft.EntityFrameworkCore;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using PlatformShared.Extentions;
using System.Buffers;

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

        public DeptService(
            MyDbContext db,
            ISEServiceApp app,
            CurrentUserAccessor userAccessor,
            ILogger<DeptService> logger,
            ICommonService commonService
        )
            : base(app, userAccessor.UserSafe, "dept", logger)
        {
            _db = db;
            _commonService = commonService;
        }

        private IQueryable<Person> CreateQuery(DeptListRQ rq, Func<IQueryable<Person>, IQueryable<Person>>? filters = null)
        {
            var query = _db.Depts(User.OrganizationInt).AsNoTracking()
                .QueryEtsoo(rq, (d) => d.Id, (d) => d.Status, (q) =>
                {
                    if (rq.Keyword?.Length > 1)
                    {
                        var keyword = rq.Keyword;

                        if (keyword.IsComplexQueryKeywords())
                        {
                            q = q.QueryEtsooKeywords(keyword, DbUtils.ILikeMethod, d => d.Name);
                        }
                        else
                        {
                            q = q.Where(d => EF.Functions.ILike(d.Name, $"%{keyword}%")
                            || (d.QueryKeyword != null && EF.Functions.ILike(d.QueryKeyword, $"%{keyword}%"))
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
            await CreateQuery(rq)
                .Select(d => new DeptQueryData
                {
                    Id = d.Id,
                    Name = d.Name
                }).ToJsonAsync(writer, cancellationToken: cancellationToken);
        }
    }
}