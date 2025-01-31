using com.etsoo.CoreFramework.User;
using PlatformShared.Messages;
using System.Globalization;

namespace PlatformShared.Extentions
{
    /// <summary>
    /// Shared extentions
    /// 共享扩展
    /// </summary>
    public static class SharedExtentions
    {
        /// <summary>
        /// Create common message data
        /// 创建通用消息数据
        /// </summary>
        /// <param name="user">Current user</param>
        /// <returns>Result</returns>
        public static CommonMessageData CreateMessageData(this CurrentUser user)
        {
            return new CommonMessageData
            {
                Culture = CultureInfo.CurrentCulture.Name,
                DeviceId = user.DeviceIdInt,
                IP = user.ClientIp.ToString(),
                UserId = user.IdInt,
                UserName = user.Name,
                OrganizationId = user.OrganizationInt,
                TimeZone = (user.TimeZone ?? TimeZoneInfo.Local).Id
            };
        }
    }
}
