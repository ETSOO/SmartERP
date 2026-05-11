using System.Text.Json.Serialization;

namespace CRM.Server.RQ.POLine
{
    /// <summary>
    /// Purchase line list request data
    /// 采购行列表请求数据
    /// </summary>
    [JsonDerivedType(typeof(POLineQueryRQ))]
    [JsonDerivedType(typeof(POLineQueryAllRQ))]
    public record POLineListRQ : QueryLongRQ
    {
        /// <summary>
        /// Purchase order id
        /// 采购订单编号
        /// </summary>
        public long? POId { get; init; }

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
