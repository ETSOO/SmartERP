using com.etsoo.CoreFramework.Business;
using System.Text.Json.Serialization;

namespace CRM.Server.RQ.OrderLine
{
    /// <summary>
    /// Order line list request data
    /// 订单行列表请求数据
    /// </summary>
    [JsonDerivedType(typeof(OrderLineQueryRQ))]
    [JsonDerivedType(typeof(OrderLineQueryAllRQ))]
    [JsonDerivedType(typeof(OrderLineQueryAssetRQ))]
    public record OrderLineListRQ : QueryLongRQ
    {
        /// <summary>
        /// Order id
        /// 订单编号
        /// </summary>
        public long? OrderId { get; init; }

        /// <summary>
        /// Supplier id
        /// 供应商编号
        /// </summary>
        public long? SupplierId { get; init; }

        /// <summary>
        /// Product id
        /// 产品编号
        /// </summary>
        public int? ProductId { get; init; }

        /// <summary>
        /// Asset id
        /// 资产编号
        /// </summary>
        public int? AssetId { get; init; }

        /// <summary>
        /// Has BOM line id or not
        /// 是否有BOM行编号
        /// </summary>
        public bool? HasBomId { get; init; }

        /// <summary>
        /// BOM line id
        /// BOM行编号
        /// </summary>
        public long? BomId { get; init; }
    }
}
