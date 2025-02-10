using com.etsoo.CoreFramework.Application;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;

namespace Platform.Server.Endpoints.App.RQ
{
    /// <summary>
    /// Application buy request data
    /// 购买应用请求数据
    /// </summary>
    public record AppBuyRQ : IModelValidator
    {
        /// <summary>
        /// Application ID
        /// 应用编号
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Months
        /// 月数
        /// </summary>
        public int? Months { get; init; }

        /// <summary>
        /// Organization ID
        /// 机构编号
        /// </summary>
        public int OrganizationId { get; init; }

        /// <summary>
        /// Validate the model
        /// 验证模块
        /// </summary>
        /// <returns>Result</returns>
        public IActionResult? Validate()
        {
            if (Id < 3)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Id));
            }

            if (OrganizationId < 1)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(OrganizationId));
            }

            if (Months.HasValue && Months.Value < 1)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Months));
            }

            return null;
        }
    }
}
