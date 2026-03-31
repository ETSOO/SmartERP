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
