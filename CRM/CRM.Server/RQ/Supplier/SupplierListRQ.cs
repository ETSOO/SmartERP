using System.Text.Json.Serialization;

namespace CRM.Server.RQ.Supplier
{
    /// <summary>
    /// Supplier list request data
    /// 供应商列表请求数据
    /// </summary>
    [JsonDerivedType(typeof(SupplierQueryRQ))]
    public record SupplierListRQ : QueryLongRQ
    {

    }
}
