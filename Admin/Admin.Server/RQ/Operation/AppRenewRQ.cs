using com.etsoo.CoreFramework.Application;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;

namespace Admin.Server.RQ.Operation
{
    /// <summary>
    /// Application renew request data
    /// 应用续费请求数据
    /// </summary>
    public record AppRenewRQ : IModelValidator
    {
        /// <summary>
        /// Organization application ID
        /// 机构应用编号
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Months to review
        /// 续费月数
        /// </summary>
        public int Months { get; init; }

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
            if (Id < 1)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Id));
            }

            if (Months == 0 || Math.Abs(Months) > 1200)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Months));
            }

            if (Comment.Length is not > 0 and < 256)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Comment));
            }

            return null;
        }
    }
}
