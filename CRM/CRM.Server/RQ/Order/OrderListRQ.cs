using System.Text.Json.Serialization;

namespace CRM.Server.RQ.Order
{
    /// <summary>
    /// Order list request data
    /// 订单列表请求数据
    /// </summary>
    [JsonDerivedType(typeof(OrderQueryRQ))]
    public record OrderListRQ : QueryLongRQ
    {

    }
}
