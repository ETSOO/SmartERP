using com.etsoo.CoreFramework.User;
using Microsoft.EntityFrameworkCore;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using PlatformShared.Messages;

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
        /// <param name="appId">Application id</param>
        /// <param name="targetId">Target id</param>
        /// <param name="targetName">Target name</param>
        /// <returns>Result</returns>
        public static CommonMessageData CreateMessageData(this CurrentUser user, int appId, long targetId, string? targetName = null)
        {
            return new CommonMessageData
            {
                AppId = appId,
                Culture = user.Language.Name,
                DeviceId = user.DeviceIdInt,
                IP = user.ClientIp.ToString(),
                UserId = user.IdInt,
                UserName = user.Name,
                OrganizationId = user.OrganizationInt,
                TimeZone = (user.TimeZone ?? TimeZoneInfo.Local).Id,
                TargetId = targetId,
                TargetName = targetName
            };
        }

        /// <summary>
        /// Query user identifiers by type and ids
        /// 通过类型和编号查询用户唯一信息
        /// </summary>
        /// <param name="db">EF db</param>
        /// <param name="type">Type</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="ids">Ids</param>
        /// <returns>Result</returns>
        public static async Task<string[][]> QueryUserIdentifiersAsync(this MyDbContext db, CoreUserIdentifierType type, CancellationToken cancellationToken = default, params IEnumerable<int>[] ids)
        {
            // Flatten all ids into a single list to query the database in one go
            var allIds = ids.SelectMany(i => i).Distinct().ToList();

            // Query CoreUserIdentifiers for all the ids in one go
            var userIdentifiers = await db.CoreUserIdentifiers
                .AsNoTracking()
                .Where(i => allIds.Contains(i.CoreUserId) && i.Type == type)
                .Select(i => new { i.CoreUserId, i.Value })
                .ToListAsync(cancellationToken);

            // Initialize the result array
            var result = new string[ids.Length][];

            for (var i = 0; i < ids.Length; i++)
            {
                var currentIds = ids[i];

                // Filter
                var emails = userIdentifiers
                    .Where(i => currentIds.Contains(i.CoreUserId))
                    .Select(i => i.Value)
                    .ToArray();

                // Assign the filtered emails to the result array
                result[i] = emails;
            }

            // Return
            return result;
        }
    }
}
