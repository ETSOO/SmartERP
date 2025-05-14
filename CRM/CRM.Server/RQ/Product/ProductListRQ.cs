using System.Text.Json.Serialization;

namespace CRM.Server.RQ.Product
{
    /// <summary>
    /// Product list request data
    /// 产品列表请求数据
    /// </summary>
    [JsonDerivedType(typeof(ProductQueryRQ))]
    public record ProductListRQ : QueryLongRQ
    {

    }
}
