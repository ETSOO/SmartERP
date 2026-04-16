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
    public record OrderLineListRQ : QueryLongRQ
    {
        /// <summary>
        /// Order id
        /// 订单编号
        /// </summary>
        public long? OrderId { get; init; }

        /// <summary>
        /// Product id
        /// 产品编号
        /// </summary>
        public int? ProductId { get; init; }
    }
}
