using com.etsoo.CoreFramework.Application;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;

namespace Platform.Server.Endpoints.Public.RQ
{
    /// <summary>
    /// Accept member invitation request data
    /// 接受成员邀请请求数据
    /// </summary>
    public record AcceptInvitationRQ : IModelValidator
    {
        /// <summary>
        /// Code id
        /// 验证码编号
        /// </summary>
        public required Guid Id { get; init; }

        /// <summary>
        /// Code to verify
        /// 验证码
        /// </summary>
        public required string Code { get; init; }

        /// <summary>
        /// Validate the model
        /// 验证模块
        /// </summary>
        /// <returns>Result</returns>
        public IActionResult? Validate()
        {
            if (Code.Length is not >= 8 and <= 128)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Code));
            }

            return null;
        }
    }
}
