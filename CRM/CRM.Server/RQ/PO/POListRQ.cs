using System.Text.Json.Serialization;

namespace CRM.Server.RQ.PO
{
    /// <summary>
    /// Purchase order list request data
    /// 采购列表请求数据
    /// </summary>
    [JsonDerivedType(typeof(POQueryRQ))]
    public record POListRQ : QueryLongRQ
    {

    }
}
