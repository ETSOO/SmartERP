using com.etsoo.CoreFramework.Application;
using com.etsoo.Utils.Actions;

namespace Platform.Server.Endpoints.Member.RQ
{
    /// <summary>
    /// Member query request data
    /// 成员查询请求数据
    /// </summary>
    public record MemberQueryRQ : MemberListRQ
    {
        /// <summary>
        /// Assigned id
        /// 分配的编号
        /// </summary>
        public string? AssignedId { get; init; }

        public override IActionResult? Validate()
        {
            var result = base.Validate();
            if (result != null)
            {
                return result;
            }

            if (AssignedId != null && AssignedId.Length > 20)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(AssignedId));
            }

            return null;
        }
    }
}
