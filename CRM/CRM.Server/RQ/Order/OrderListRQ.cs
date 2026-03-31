using CRM.Server.Dto;
using System.Text.Json.Serialization;

namespace CRM.Server.RQ.Order
{
    /// <summary>
    /// Order list request data
    /// 订单列表请求数据
    /// </summary>
    [JsonDerivedType(typeof(OrderQueryRQ))]
    public record OrderListRQ : QueryLongRQ, IQueryTag
    {
        /// <summary>
        /// Tag
        /// 标签
        /// </summary>
        public string? Tag { get; init; }

        /// <summary>
        /// Tag ID
        /// 标签编号
        /// </summary>
        public int? TagId { get; set; }

        /// <summary>
        /// Source
        /// 来源
        /// </summary>
        public string? Source { get; init; }

        /// <summary>
        /// Customer id
        /// 客户编号
        /// </summary>
        public long? CustomerId { get; init; }

        /// <summary>
        /// Currency
        /// 币种
        /// </summary>
        public string? Currency { get; init; }

        /// <summary>
        /// Culture
        /// 文化
        /// </summary>
        public string? Culture { get; init; }

        /// <summary>
        /// Delivery id
        /// 配送方式编号
        /// </summary>
        public int? DeliveryId { get; init; }

        /// <summary>
        /// Payment id
        /// 付款方式
        /// </summary>
        public int? PaymentId { get; init; }

        /// <summary>
        /// Product id
        /// 产品编号
        /// </summary>
        public int? ProductId { get; init; }

        /// <summary>
        /// User id
        /// 用户编号
        /// </summary>
        public long? UserId { get; init; }
    }
}
