using System.Text.Json.Serialization;

namespace CRM.Server.RQ.Customer
{
    /// <summary>
    /// Customer list request data
    /// 客户列表请求数据
    /// </summary>
    [JsonDerivedType(typeof(CustomerQueryRQ))]
    public record CustomerListRQ : QueryLongRQ
    {

    }
}
