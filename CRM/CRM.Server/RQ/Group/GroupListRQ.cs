using System.Text.Json.Serialization;

namespace CRM.Server.RQ.Group
{
    /// <summary>
    /// Permission group list request data
    /// 权限组列表请求数据
    /// </summary>
    [JsonDerivedType(typeof(GroupQueryRQ))]
    public record GroupListRQ : QueryLongRQ
    {

    }
}
