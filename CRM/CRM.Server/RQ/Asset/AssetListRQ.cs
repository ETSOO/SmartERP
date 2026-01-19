using System.Text.Json.Serialization;

namespace CRM.Server.RQ.Asset
{
    /// <summary>
    /// Asset list request data
    /// 资产列表请求数据
    /// </summary>
    [JsonDerivedType(typeof(AssetQueryRQ))]
    public record AssetListRQ : QueryIntRQ
    {
        /// <summary>
        /// Person (owner) id
        /// 所有者编号
        /// </summary>
        public long? PersonId { get; init; }

        /// <summary>
        /// Product id
        /// 产品编号
        /// </summary>
        public int? ProductId { get; init; }

        /// <summary>
        /// Supplier id
        /// 供应商编号
        /// </summary>
        public long? SupplierId { get; init; }

        /// <summary>
        /// Serial number
        /// 序列号
        /// </summary>
        public string? Sn { get; init; }
    }
}
