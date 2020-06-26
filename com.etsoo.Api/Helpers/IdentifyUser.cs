using com.etsoo.Core.Services;
using com.etsoo.Core.Utils;
using System.Linq;
using System.Net;
using System.Security.Claims;

namespace com.etsoo.Api.Helpers
{
    /// <summary>
    /// Identify user
    /// 识别用户
    /// </summary>
    public static class IdentifyUser
    {
        /// <summary>
        /// Create user
        /// 创建用户
        /// </summary>
        /// <param name="principal">Principal</param>
        /// <param name="ipAddress">Ip address</param>
        /// <returns>Current user</returns>
        public static CurrentUser Create(ClaimsPrincipal principal, IPAddress ipAddress)
        {
            // Define the user
            var user = new CurrentUser(3, ipAddress);

            // Login if authenticated
            var identity = principal.Identity;
            if (identity.IsAuthenticated && identity.Name != null)
            {
                var userId = ParseUtil.TryParse<int>(identity.Name).GetValueOrDefault();
                var organizationId = ParseUtil.TryParse<int>(principal.Claims.FirstOrDefault(c => c.Type.Equals("OrganizationId"))?.Value).GetValueOrDefault();
                var languageCid = principal.Claims.FirstOrDefault(c => c.Type.Equals("LanguageCid"))?.Value;
                var rolesText = principal.Claims.FirstOrDefault(c => c.Type.Equals(ClaimTypes.Role))?.Value;
                var roles = string.IsNullOrEmpty(rolesText) ? new string[] { } : rolesText.Split(',');
                user.Login(userId, organizationId, languageCid, roles);
            }

            // Return
            return user;
        }
    }
}