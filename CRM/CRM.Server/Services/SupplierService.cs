using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.ServiceApp.SmartERP;
using CRM.Server.Dto.Supplier;
using CRM.Server.RQ.Supplier;
using Microsoft.EntityFrameworkCore;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using PlatformShared.Extentions;
using System.Buffers;

namespace CRM.Server.Services
{
    /// <summary>
    /// Supplier service
    /// 供应商服务
    /// </summary>
    public class SupplierService : SEUserService, ISupplierService
    {
        readonly MyDbContext _db;

        public SupplierService(
            MyDbContext db,
            ISEServiceApp app,
            CurrentUserAccessor userAccessor,
            ILogger<SupplierService> logger
        )
            : base(app, userAccessor.UserSafe, "supplier", logger)
        {
            _db = db;
        }

        private IQueryable<Person> CreateQuery(SupplierListRQ rq, Func<IQueryable<Person>, IQueryable<Person>>? filters = null)
        {
            var query = _db.Suppliers(User.OrganizationInt)
                .QueryEtsoo(rq, (c) => c.Id, (c) => c.Status, (q) =>
                {
                    if (rq.Keyword?.Length > 1)
                    {
                        var keyword = rq.Keyword;

                        if (keyword.IsComplexQueryKeywords())
                        {
                            q = q.QueryEtsooKeywords(keyword, DbUtils.ILikeMethod, c => c.Name, c => c.PreferredName);
                        }
                        else
                        {
                            q = q.Where(c => EF.Functions.ILike(c.Name, $"%{keyword}%")
                            || (c.QueryKeyword != null && EF.Functions.ILike(c.QueryKeyword, $"%{keyword}%"))
                            || (c.PreferredName != null && EF.Functions.ILike(c.PreferredName, $"%{keyword}%"))
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
        /// List supplier JSON data
        /// 供应商列表JSON数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task ListAsync(SupplierListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            await CreateQuery(rq)
                .Select(c => new SupplierListData
                {
                    Id = c.Id,
                    Name = c.Name
                }).ToJsonAsync(writer, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Query supplier JSON data
        /// 查询供应商JSON数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task QueryAsync(SupplierQueryRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            await CreateQuery(rq)
                .Select(c => new SupplierQueryData
                {
                    Id = c.Id,
                    Name = c.Name
                }).ToJsonAsync(writer, cancellationToken: cancellationToken);
        }
    }
}