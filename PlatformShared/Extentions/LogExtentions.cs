using com.etsoo.Utils.Serialization;
using PlatformShared.Database;
using PlatformShared.LogDatabase.Models;
using PlatformShared.Messages;
using System.Net;
using System.Text.Json;

namespace PlatformShared.Extentions
{
    /// <summary>
    /// Log DB extentions
    /// </summary>
    public static class LogExtentions
    {
        /// <summary>
        /// Log
        /// 记录日志
        /// </summary>
        /// <typeparam name="T">Generic message type</typeparam>
        /// <param name="logDb">Log database context</param>
        /// <param name="message">Message to log</param>
        /// <param name="title">Log title</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public static Task LogAsync<T>(this LogDbContext logDb, T message, string title, CancellationToken cancellationToken = default)
            where T : CommonMessage, IMessageQueueMessage
        {
            return LogAsync(logDb, message, title, message.Data.UserId, message.Data.OrganizationId, null, cancellationToken);
        }

        /// <summary>
        /// Log
        /// 记录日志
        /// </summary>
        /// <typeparam name="T">Generic message type</typeparam>
        /// <param name="logDb">Log database context</param>
        /// <param name="message">Message to log</param>
        /// <param name="title">Log title</param>
        /// <param name="userId">User ID</param>
        /// <param name="orgId">Organization ID</param>
        /// <param name="kind">Kind</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public static Task LogAsync<T>(this LogDbContext logDb, T message, string title, int userId, int? orgId, string? kind, CancellationToken cancellationToken = default)
            where T : CommonMessage, IMessageQueueMessage
        {
            var data = message.Data;
            var type = T.Type;

            var dic = message.GetJsonData();
            string? jsonData;

            if (dic == null || dic.Count == 0)
            {
                jsonData = message.JsonData;
            }
            else
            {
                if (!string.IsNullOrEmpty(message.JsonData))
                {
                    // Trigger exception when same name item exists
                    dic.Add(nameof(message.JsonData), message.JsonData);
                }

                jsonData = JsonSerializer.Serialize(dic, CommonJsonSerializerContext.Default.DictionaryStringObject);
            }

            var log = new CoreLog
            {
                AppId = data.AppId,
                Culture = data.Culture,
                Data = jsonData,
                DeviceId = data.DeviceId,
                Ip = IPAddress.Parse(data.IP),
                OrganizationId = orgId,
                Title = title,
                UserId = userId,
                Kind = kind ?? type,
                TargetId = data.TargetId > 0 ? data.TargetId : null,
                Creation = data.TimeStamp.ToUniversalTime()
            };

            logDb.CoreLogs.Add(log);

            return logDb.SaveChangesAsync(cancellationToken);
        }
    }
}
