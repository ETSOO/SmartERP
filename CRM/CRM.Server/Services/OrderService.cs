using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.ServiceApp.SmartERP;
using CRM.Server.Dto.Order;
using CRM.Server.RQ.Order;
using Microsoft.EntityFrameworkCore;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using PlatformShared.Extentions;
using System.Buffers;

namespace CRM.Server.Services
{
    /// <summary>
    /// Order service
    /// 订单服务
    /// </summary>
    public class OrderService : SEUserService, IOrderService
    {
        readonly MyDbContext _db;

        public OrderService(
            MyDbContext db,
            ISEServiceApp app,
            CurrentUserAccessor userAccessor,
            ILogger<OrderService> logger
        )
            : base(app, userAccessor.UserSafe, "order", logger)
        {
            _db = db;
        }

        private IQueryable<OrderHeader> CreateQuery(OrderListRQ rq, Func<IQueryable<OrderHeader>, IQueryable<OrderHeader>>? filters = null)
        {
            var query = _db.Orders(User).AsNoTracking()
                .QueryEtsoo(rq, (o) => o.Id, (o) => o.Status, (q) =>
                {
                    if (rq.Keyword?.Length > 1)
                    {
                        var keyword = rq.Keyword;

                        if (keyword.IsComplexQueryKeywords())
                        {
                            q = q.QueryEtsooKeywords(keyword, DbUtils.ILikeMethod, a => a.Title, a => a.Description);
                        }
                        else
                        {
                            q = q.Where(ou => EF.Functions.ILike(ou.Title, $"%{keyword}%")
                            || (ou.Description != null && EF.Functions.ILike(ou.Description, $"%{keyword}%"))
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
        /// List order JSON data
        /// 订单列表JSON数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task ListAsync(OrderListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            await CreateQuery(rq)
                .Select(o => new OrderListData
                {
                    Id = o.Id,
                    Title = o.Title
                }).ToJsonAsync(writer, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Query order JSON data
        /// 查询订单JSON数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task QueryAsync(OrderQueryRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            await CreateQuery(rq)
                .Select(o => new OrderQueryData
                {
                    Id = o.Id,
                    Title = o.Title
                }).ToJsonAsync(writer, cancellationToken: cancellationToken);
        }
    }
}