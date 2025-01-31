using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Business;
using com.etsoo.Utils.Actions;
using Platform.Server.Dto.Auth;

namespace Platform.Server.Services
{
    /// <summary>
    /// Service extentions
    /// 服务扩展
    /// </summary>
    public static partial class ServiceExtentions
    {
        /// <summary>
        /// Validate user
        /// 验证用户
        /// </summary>
        /// <param name="user">User</param>
        /// <returns>Result</returns>
        public static ActionResult ValidateUser(this LoginUser user, TimeZoneInfo? tz)
        {
            if (user.FrozenTime.HasValue && user.FrozenTime.Value > DateTimeOffset.UtcNow)
            {
                var result = ApplicationErrors.UserFrozen.AsResult();

                // Always send back the frozen time for the client to customize the message
                result.Data[nameof(user.FrozenTime)] = user.FrozenTime.Value;

                if (tz != null && result.Title != null)
                {
                    // Default error message
                    var targetTime = TimeZoneInfo.ConvertTime(user.FrozenTime.Value, tz);
                    var targetStr = $"{targetTime} ({tz.StandardName})";
                    result.Title = string.Format(result.Title, targetStr);
                }

                return result;
            }
            else if (user.Status > EntityStatus.Approved)
            {
                return ApplicationErrors.AccountDisabled.AsResult("Status");
            }
            else if (user.OrgStatus != null && user.OrgStatus > EntityStatus.Approved)
            {
                return ApplicationErrors.AccountDisabled.AsResult("OrgStatus");
            }
            else if (user.OrgExpiry != null && user.OrgExpiry < DateTime.UtcNow)
            {
                return ApplicationErrors.AccountDisabled.AsResult("OrgExpiry");
            }
            else
            {
                return ActionResult.Success;
            }
        }
    }
}
