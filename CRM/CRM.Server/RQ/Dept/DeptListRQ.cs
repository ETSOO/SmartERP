using System.Text.Json.Serialization;

namespace CRM.Server.RQ.Dept
{
    /// <summary>
    /// Department list request data
    /// 部门列表请求数据
    /// </summary>
    [JsonDerivedType(typeof(DeptQueryRQ))]
    public record DeptListRQ : QueryLongRQ
    {
        /// <summary>
        /// Leader
        /// 部门主管
        /// </summary>
        public long? LeaderId { get; init; }
    }
}
