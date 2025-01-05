using com.etsoo.CoreFramework.Application;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;

namespace Platform.Server.Endpoints.App.RQ
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

            if (Months == 0 ||  Math.Abs(Months) > 1200)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Months));
            }

            return null;
        }
    }
}
