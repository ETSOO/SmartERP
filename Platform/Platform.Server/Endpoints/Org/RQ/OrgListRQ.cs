using System.Text.Json.Serialization;

namespace Platform.Server.Endpoints.Org.RQ
{
    /// <summary>
    /// Organization list request data
    /// 机构列表请求数据
    /// </summary>
    [JsonDerivedType(typeof(OrgQueryRQ))]
    public record OrgListRQ : QueryIntRQ
    {
        /// <summary>
        /// Parent org. ID
        /// 父机构编号
        /// </summary>
        public int? ParentId { get; init; }
    }
}
