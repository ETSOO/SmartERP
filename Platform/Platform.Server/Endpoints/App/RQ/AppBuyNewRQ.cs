using com.etsoo.CoreFramework.Application;
using com.etsoo.Database.Converters;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;

namespace Platform.Server.Endpoints.App.RQ
{
    /// <summary>
    /// Application buy and creating new organization request data
    /// 购买应用并创建新机构请求数据
    /// </summary>
    public record AppBuyNewRQ : IModelValidator
    {
        /// <summary>
        /// Application ID
        /// 应用编号
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// New organization name
        /// 新机构名称
        /// </summary>
        public required string OrgName { get; init; }

        /// <summary>
        /// New organization PIN
        /// 新机构编号
        /// </summary>
        public string? OrgPin { get; init; }

        /// <summary>
        /// Region
        /// 所在地区
        /// </summary>
        public required string Region { get; init; }

        /// <summary>
        /// Time zone
        /// 时区
        /// </summary>
        public string? TimeZone { get; init; }

        /// <summary>
        /// Validate the model
        /// 验证模块
        /// </summary>
        /// <returns>Result</returns>
        public IActionResult? Validate()
        {
            if (OrgName.Length is not (>= 2 and <= 128))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(OrgName));
            }

            if (OrgPin != null && OrgPin.Length is not (>= 6 and <= 20))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(OrgPin));
            }

            if (Region.Length is not 2)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Region));
            }

            if (TimeZone != null && !TimeZoneUtils.IsTimeZone(TimeZone))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(TimeZone));
            }

            return null;
        }
    }
}
