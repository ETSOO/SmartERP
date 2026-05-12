using PlatformShared.Dto;
using System.Text.Json.Serialization;

namespace CRM.Server.RQ.Stock
{
    /// <summary>
    /// Stock list request
    /// 库存列表请求
    /// </summary>
    [JsonDerivedType(typeof(StockQueryRQ))]
    public record StockListRQ : QueryLongRQ
    {
        /// <summary>
        /// Kind
        /// 类型
        /// </summary>
        public StockKind? Kind { get; init; }

        /// <summary>
        /// Customer / supplier id
        /// 客户 / 供应商 编号
        /// </summary>
        public long? PersonId { get; init; }

        /// <summary>
        /// Shipping address id
        /// 发货地址编号
        /// </summary>
        public int? LocationFromId { get; init; }

        /// <summary>
        /// Receiving address id
        /// 收货地址编号
        /// </summary>
        public int? LocationToId { get; init; }

        /// <summary>
        /// User id
        /// 操作用户编号
        /// </summary>
        public long? UserId { get; init; }

        /// <summary>
        /// Order / PO id
        /// 订单 / 采购编号
        /// </summary>
        public long? OrderId { get; init; }

        /// <summary>
        /// Tracking number
        /// 物流编号
        /// </summary>
        public string? TrackingNumber { get; init; }

        /// <summary>
        /// Is in transit
        /// 是否在途
        /// </summary>
        public bool? Intransit { get; init; }
    }
}
