using com.etsoo.MessageQueue;
using com.etsoo.MessageQueue.QueueProcessors;
using PlatformShared.Database;
using PlatformShared.LogDatabase.Models;
using PlatformShared.Messages;
using System.Globalization;
using System.Net;
using System.Text.Json.Serialization.Metadata;

namespace WorkerCenter.Main.Processors
{
    /// <summary>
    /// Log queue processor
    /// 日志队列处理器
    /// </summary>
    /// <typeparam name="T">Generic message type</typeparam>
    public abstract class LogQueueProcessor<T> : CommonQueueProcessor<T> where T : CommonMessage, IMessageQueueMessage
    {
        private readonly LogDbContext _logDb;

        /// <summary>
        /// Log database context
        /// </summary>
        protected LogDbContext LogDb => _logDb;

        protected LogQueueProcessor(ILogger logger, JsonTypeInfo<T> typeInfo, LogDbContext logDb)
            : base(logger, typeInfo)
        {
            _logDb = logDb;
        }

        protected override async Task ProcessMessageAsync(T message, MessageReceivedProperties properties, CancellationToken cancellationToken)
        {
            var data = message.Data;
            var type = T.Type;

            var ci = CultureInfo.GetCultureInfo(data.Culture);
            var title = Properties.Resources.ResourceManager.GetString(type, ci) ?? type;

            var log = new CoreLog
            {
                Culture = data.Culture,
                Data = message.GetMoreData(),
                DeviceId = data.DeviceId,
                Ip = IPAddress.Parse(data.IP),
                OrganizationId = data.OrganizationId,
                Title = title,
                UserId = data.UserId,
                Kind = type
            };

            _logDb.CoreLogs.Add(log);

            await _logDb.SaveChangesAsync(cancellationToken);
        }
    }
}
