using PlatformShared.Database.Models;
using System.Text.Json.Serialization;

namespace CRM.Server.RQ.OrderPayment
{
    /// <summary>
    /// Order payment list request data
    /// 订单支付方式列表请求数据
    /// </summary>
    [JsonDerivedType(typeof(OrderPaymentQueryRQ))]
    public record OrderPaymentListRQ : QueryIntRQ
    {
        /// <summary>
        /// Is order or not
        /// 是否为订单
        /// </summary>
        public bool IsOrder { get; init; }

        /// <summary>
        /// Kind
        /// 类型
        /// </summary>
        public OrderPaymentKind? Kind { get; init; }

        /// <summary>
        /// Is valid
        /// 是否有效
        /// </summary>
        public bool? IsValid { get; init; }
    }
}
