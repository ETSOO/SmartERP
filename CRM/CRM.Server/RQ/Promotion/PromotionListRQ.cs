using System.Text.Json.Serialization;

namespace CRM.Server.RQ.Promotion
{
    /// <summary>
    /// Promotion list request data
    /// 促销列表请求数据
    /// </summary>
    [JsonDerivedType(typeof(PromotionQueryRQ))]
    public record PromotionListRQ : QueryIntRQ
    {
        /// <summary>
        /// Code
        /// 代码
        /// </summary>
        public short? Code { get; init; }

        /// <summary>
        /// Currency
        /// 币种
        /// </summary>
        public string? Currency { get; init; }

        /// <summary>
        /// Is valid
        /// 是否有效
        /// </summary>
        public bool? IsValid { get; init; }

        /// <summary>
        /// Person (customer) id
        /// 人员（客户）编号
        /// </summary>
        public long? PersonId { get; init; }

        /// <summary>
        /// Product included
        /// 包含的产品
        /// </summary>
        public int? ProductId { get; init; }

        /// <summary>
        /// Stackable
        /// 是否可叠加
        /// </summary>
        public bool? Stackable { get; init; }
    }
}
