using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.Utils.Actions;
using CRM.Server.Application;
using CRM.Server.Dto.OrderPayment;
using CRM.Server.RQ.OrderPayment;
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
    /// Order Payment Service
    /// 订单支付方式服务
    /// </summary>
    public class OrderPaymentService : MyUserService, IOrderPaymentService
    {
        readonly MyDbContext _db;
        readonly ICommonService _commonService;
        readonly IQueueService _queueService;

        public OrderPaymentService(
            MyDbContext db,
            IMyApp app,
            MyAppConfiguration config,
            CurrentUserAccessor userAccessor,
            ILogger<OrderPaymentService> logger,
            ICommonService commonService,
            IQueueService queueService
        )
            : base(app, config, userAccessor.UserSafe, "order_payment", logger)
        {
            _db = db;
            _commonService = commonService;
            _queueService = queueService;
        }

        private IQueryable<OrderPayment> CreateQuery(OrderPaymentListRQ rq)
        {
            var orgId = User.OrganizationInt;

            var query = _db.OrderPayments(orgId).AsNoTracking()
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
                        q = q.Where(p => EF.Functions.ILike(p.Title, $"%{keyword}%") || (p.Description != null && EF.Functions.ILike(p.Description, $"%{keyword}%")));
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
        public async Task<IActionResult> CreateAsync(OrderPaymentCreateRQ rq, CancellationToken cancellationToken = default)
        {
            if (!await _commonService.HasPermissionAsync((short)Permissions.Order.Manage, cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            var orgId = User.OrganizationInt;

            var payment = new OrderPayment
            {
                CoreOrganizationId = orgId,
                Kind = rq.Kind,
                Title = rq.Title,
                Description = rq.Description,
                IsOrder = rq.IsOrder,
                IsValid = rq.IsValid.GetValueOrDefault(true),
                OrderIndex = rq.OrderIndex.GetValueOrDefault()
            };

            _db.OrderPayments.Add(payment);

            await _db.SaveChangesAsync(cancellationToken);

            var id = payment.Id;

            // Push message
            var message = new CreateOrderPaymentMessage
            {
                Data = User.CreateMessageData(App.AppId, id, payment.Title),
                JsonData = JsonSerializer.Serialize(rq, MyJsonSerializerContext.Default.OrderPaymentCreateRQ)
            };
            await _queueService.PushAsync(message, CrmJsonSerializerContext.Default.CreateOrderPaymentMessage, cancellationToken);

            return ActionResult.Succeed(id);
        }

        /// <summary>
        /// List order payment JSON data
        /// 订单支付方式列表JSON数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public Task ListAsync(OrderPaymentListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            return CreateQuery(rq)
                .OrderBy(p => p.OrderIndex)
                .ThenBy(p => p.Id)
                .Select(p => new OrderPaymentListData
                {
                    Id = p.Id,
                    Title = p.Title
                }).ToJsonAsync(writer, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Query order payment
        /// 查询订单支付方式
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public Task<OrderPaymentQueryData[]> QueryAsync(OrderPaymentQueryRQ rq, CancellationToken cancellationToken = default)
        {
            return CreateQuery(rq)
                .TagWith(nameof(QueryAsync))
                .Select(p => new OrderPaymentQueryData
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
                UPDATE "order_payment"
                    SET "order_index" = t."sorder_index"
                FROM (VALUES {string.Join(", ", ids.Select((id, i) => $"({id}, {indices[i]})"))}) AS t("sid", "sorder_index")
                WHERE "core_organization_id" = {orgId} AND "id" = t."sid";
            """, cancellationToken);
#pragma warning restore EF1002 // No risk of vulnerability to SQL injection.

            // Push message
            var message = new SortOrderPaymentMessage
            {
                Data = User.CreateMessageData(App.AppId, 0),
                JsonData = JsonSerializer.Serialize(rq, MyJsonSerializerContext.Default.DictionaryInt32Int16)
            };
            var task2 = _queueService.PushAsync(message, CrmJsonSerializerContext.Default.SortOrderPaymentMessage, cancellationToken);

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
        public async Task<IActionResult> UpdateAsync(OrderPaymentUpdateRQ rq, CancellationToken cancellationToken = default)
        {
            if (!await _commonService.HasPermissionAsync((short)Permissions.Order.Manage, cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            var orgId = User.OrganizationInt;

            var payment = await _db.OrderPayments(orgId)
                .Where(p => p.Id == rq.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (payment == null)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            if (rq.IsModified(nameof(rq.Kind)) && rq.Kind.HasValue)
            {
                payment.Kind = rq.Kind.Value;
            }

            if (rq.IsModified(nameof(rq.Title)) && rq.Title != null)
            {
                payment.Title = rq.Title;
            }

            if (rq.IsModified(nameof(rq.Description)))
            {
                payment.Description = rq.Description;
            }

            if (rq.IsModified(nameof(rq.IsValid)) && rq.IsValid.HasValue)
            {
                payment.IsValid = rq.IsValid.Value;
            }

            if (rq.IsModified(nameof(rq.OrderIndex)) && rq.OrderIndex.HasValue)
            {
                payment.OrderIndex = rq.OrderIndex.Value;
            }

            // Changes
            var changes = _db.ChangeTracker.Entries().GetChangedProperties();

            var task1 = _db.SaveChangesAsync(cancellationToken);

            // Push message
            var message = new UpdateOrderPaymentMessage
            {
                Data = User.CreateMessageData(App.AppId, rq.Id, payment.Title),
                Changes = changes
            };
            var task2 = _queueService.PushAsync(message, CrmJsonSerializerContext.Default.UpdateOrderPaymentMessage, cancellationToken);

            await Task.WhenAll(task1, task2);

            return ActionResult.Succeed(rq.Id);
        }

        /// <summary>
        /// Read data for update
        /// 读取用于更新的数据
        /// </summary>
        /// <param name="id">Order payment id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<OrderPaymentUpdateReadData?> UpdateReadAsync(int id, CancellationToken cancellationToken = default)
        {
            if (!await _commonService.HasPermissionAsync((short)Permissions.Order.Manage, cancellationToken))
            {
                return null;
            }

            var orgId = User.OrganizationInt;

            return await _db.OrderPayments(orgId).AsNoTracking()
                .Where(p => p.Id == id)
                .Select(p => new OrderPaymentUpdateReadData
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
