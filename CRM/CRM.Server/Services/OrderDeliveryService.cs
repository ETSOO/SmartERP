using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.ServiceApp.SmartERP;
using com.etsoo.Utils.Actions;
using CRM.Server.Dto.OrderDelivery;
using CRM.Server.RQ.OrderDelivery;
using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Order;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using PlatformShared.Extentions;
using System.Buffers;
using System.Text.Json;

namespace CRM.Server.Services
{
    /// <summary>
    /// Order Delivery Service
    /// 订单配送方式服务
    /// </summary>
    public class OrderDeliveryService : SEUserService, IOrderDeliveryService
    {
        readonly MyDbContext _db;
        readonly ICommonService _commonService;
        readonly IQueueService _queueService;

        public OrderDeliveryService(
            MyDbContext db,
            ISEServiceApp app,
            CurrentUserAccessor userAccessor,
            ILogger<OrderDeliveryService> logger,
            ICommonService commonService,
            IQueueService queueService
        )
            : base(app, userAccessor.UserSafe, "order_delivery", logger)
        {
            _db = db;
            _commonService = commonService;
            _queueService = queueService;
        }

        private IQueryable<OrderDelivery> CreateQuery(OrderDeliveryListRQ rq)
        {
            var orgId = User.OrganizationInt;

            var query = _db.OrderDeliveries(orgId).AsNoTracking()
                .QueryEtsoo(rq, (p) => p.Id, null, (q) =>
                {
                    q = q.Where(p => p.IsOrder == rq.IsOrder);

                    if (rq.Kind.HasValue)
                    {
                        q = q.Where(p => p.Kind == rq.Kind.Value);
                    }

                    if (rq.IsValid.HasValue)
                    {
                        q = q.Where(p => p.IsValid == rq.IsValid.Value);
                    }

                    if (rq.Keyword?.Length > 1)
                    {
                        var keyword = rq.Keyword;
                        q = q.Where(d => EF.Functions.ILike(d.Title, $"%{keyword}%") || (d.Description != null && EF.Functions.ILike(d.Description, $"%{keyword}%")));
                    }

                    return q;
                });

            return query;
        }

        /// <summary>
        /// Create
        /// 创建
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> CreateAsync(OrderDeliveryCreateRQ rq, CancellationToken cancellationToken = default)
        {
            if (!await _commonService.HasPermissionAsync((short)Permissions.Order.Manage, cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            var orgId = User.OrganizationInt;

            var delivery = new OrderDelivery
            {
                CoreOrganizationId = orgId,
                Kind = rq.Kind,
                Title = rq.Title,
                Description = rq.Description,
                IsOrder = rq.IsOrder,
                IsValid = rq.IsValid.GetValueOrDefault(true),
                OrderIndex = rq.OrderIndex.GetValueOrDefault()
            };

            _db.OrderDeliveries.Add(delivery);

            await _db.SaveChangesAsync(cancellationToken);

            var id = delivery.Id;

            // Push message
            var message = new CreateOrderDeliveryMessage
            {
                Data = User.CreateMessageData(App.AppId, id, delivery.Title),
                JsonData = JsonSerializer.Serialize(rq, MyJsonSerializerContext.Default.OrderDeliveryCreateRQ)
            };
            await _queueService.PushAsync(message, CrmJsonSerializerContext.Default.CreateOrderDeliveryMessage, cancellationToken);

            return ActionResult.Succeed(id);
        }

        /// <summary>
        /// List order delivery JSON data
        /// 订单配送方式列表JSON数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public Task ListAsync(OrderDeliveryListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            return CreateQuery(rq)
                .OrderBy(c => c.OrderIndex)
                .ThenBy(c => c.Id)
                .Select(p => new OrderDeliveryListData
                {
                    Id = p.Id,
                    Title = p.Title
                }).ToJsonAsync(writer, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Query order delivery
        /// 查询订单配送方式
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public Task<OrderDeliveryQueryData[]> QueryAsync(OrderDeliveryQueryRQ rq, CancellationToken cancellationToken = default)
        {
            return CreateQuery(rq)
                .TagWith(nameof(QueryAsync))
                .Select(p => new OrderDeliveryQueryData
                {
                    Id = p.Id,
                    Kind = p.Kind,
                    Title = p.Title,
                    Description = p.Description,
                    IsValid = p.IsValid
                })
                .ToArrayAsync(cancellationToken);
        }

        /// <summary>
        /// Sort
        /// 排序
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<int> SortAsync(Dictionary<int, short> rq, CancellationToken cancellationToken = default)
        {
            if (!await _commonService.HasPermissionAsync((short)Permissions.Order.Manage, cancellationToken))
            {
                return -1;
            }

            var orgId = User.OrganizationInt;

            var ids = rq.Keys.ToArray();
            var indices = rq.Values.ToArray();

#pragma warning disable EF1002 // No risk of vulnerability to SQL injection.
            var task1 = _db.Database.ExecuteSqlRawAsync($"""
                UPDATE "order_delivery"
                    SET "order_index" = t."sorder_index"
                FROM (VALUES {string.Join(", ", ids.Select((id, i) => $"({id}, {indices[i]})"))}) AS t("sid", "sorder_index")
                WHERE "core_organization_id" = {orgId} AND "id" = t."sid";
            """, cancellationToken);
#pragma warning restore EF1002 // No risk of vulnerability to SQL injection.

            // Push message
            var message = new SortOrderDeliveryMessage
            {
                Data = User.CreateMessageData(App.AppId, 0),
                JsonData = JsonSerializer.Serialize(rq, MyJsonSerializerContext.Default.DictionaryInt32Int16)
            };
            var task2 = _queueService.PushAsync(message, CrmJsonSerializerContext.Default.SortOrderDeliveryMessage, cancellationToken);

            await Task.WhenAll(task1, task2);

            return task1.Result;
        }

        /// <summary>
        /// Update
        /// 更新
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> UpdateAsync(OrderDeliveryUpdateRQ rq, CancellationToken cancellationToken = default)
        {
            if (!await _commonService.HasPermissionAsync((short)Permissions.Order.Manage, cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            var orgId = User.OrganizationInt;

            var delivery = await _db.OrderDeliveries(orgId)
                .Where(p => p.Id == rq.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (delivery == null)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            if (rq.IsModified(nameof(rq.Kind)) && rq.Kind.HasValue)
            {
                delivery.Kind = rq.Kind.Value;
            }

            if (rq.IsModified(nameof(rq.Title)) && rq.Title != null)
            {
                delivery.Title = rq.Title;
            }

            if (rq.IsModified(nameof(rq.Description)))
            {
                delivery.Description = rq.Description;
            }

            if (rq.IsModified(nameof(rq.IsValid)) && rq.IsValid.HasValue)
            {
                delivery.IsValid = rq.IsValid.Value;
            }

            if (rq.IsModified(nameof(rq.OrderIndex)) && rq.OrderIndex.HasValue)
            {
                delivery.OrderIndex = rq.OrderIndex.Value;
            }

            // Changes
            var changes = _db.ChangeTracker.Entries().GetChangedProperties();

            var task1 = _db.SaveChangesAsync(cancellationToken);

            // Push message
            var message = new UpdateOrderDeliveryMessage
            {
                Data = User.CreateMessageData(App.AppId, rq.Id, delivery.Title),
                Changes = changes
            };
            var task2 = _queueService.PushAsync(message, CrmJsonSerializerContext.Default.UpdateOrderDeliveryMessage, cancellationToken);

            await Task.WhenAll(task1, task2);

            return ActionResult.Succeed(rq.Id);
        }

        /// <summary>
        /// Read data for update
        /// 读取用于更新的数据
        /// </summary>
        /// <param name="id">Order delivery id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<OrderDeliveryUpdateReadData?> UpdateReadAsync(int id, CancellationToken cancellationToken = default)
        {
            if (!await _commonService.HasPermissionAsync((short)Permissions.Order.Manage, cancellationToken))
            {
                return null;
            }

            var orgId = User.OrganizationInt;

            return await _db.OrderDeliveries(orgId).AsNoTracking()
                .Where(p => p.Id == id)
                .Select(p => new OrderDeliveryUpdateReadData
                {
                    Id = p.Id,
                    Kind = p.Kind,
                    Title = p.Title,
                    Description = p.Description,
                    IsOrder = p.IsOrder,
                    IsValid = p.IsValid,
                    OrderIndex = p.OrderIndex
                }).FirstOrDefaultAsync(cancellationToken);
        }
    }
}
