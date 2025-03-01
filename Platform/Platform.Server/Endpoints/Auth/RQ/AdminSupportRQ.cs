using com.etsoo.CoreFramework.Application;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;

namespace Platform.Server.Endpoints.Auth.RQ
{
    /// <summary>
    /// Admin support request data
    /// 管理员支持请求数据
    /// </summary>
    public record AdminSupportRQ : IModelValidator
    {
        /// <summary>
        /// Target organization ID
        /// 目标机构编号
        /// </summary>
        public required int OrgId { get; init; }

        /// <summary>
        /// Requester
        /// 请求人
        /// </summary>
        public required int Requester { get; init; }

        /// <summary>
        /// Approver
        /// 批准人
        /// </summary>
        public required int Approver { get; init; }

        /// <summary>
        /// Comment
        /// 备注
        /// </summary>
        public required string Comment { get; init; }

        /// <summary>
        /// Validate the model
        /// 验证模块
        /// </summary>
        /// <returns>Result</returns>
        public IActionResult? Validate()
        {
            if (OrgId < 1)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(OrgId));
            }

            if (Approver < 1 || Requester < 1 || Approver == Requester)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Approver));
            }

            if (Comment.Length is not > 0 and < 256)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Comment));
            }

            return null;
        }
    }
}
