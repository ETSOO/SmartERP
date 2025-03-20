using System.Text.Json.Serialization;

namespace CRM.Server.RQ.Person
{
    /// <summary>
    /// Person list request data
    /// 人员列表请求数据
    /// </summary>
    [JsonDerivedType(typeof(PersonQueryRQ))]
    public record PersonListRQ : QueryLongRQ
    {

    }
}
