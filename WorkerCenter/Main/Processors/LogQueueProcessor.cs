using com.etsoo.Localization;
using com.etsoo.MessageQueue;
using com.etsoo.MessageQueue.QueueProcessors;
using com.etsoo.Utils.Serialization;
using PlatformShared.Database;
using PlatformShared.Extentions;
using PlatformShared.Messages;
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

        /// <summary>
        /// Get log title
        /// 获取日志标题
        /// </summary>
        /// <param name="message">Current message</param>
        /// <returns>Result</returns>
        protected virtual string GetLogTitle(T message)
        {
            var type = T.Type;
            var label = Properties.Resources.ResourceManager.GetString(type) ?? type;

            if (string.IsNullOrEmpty(message.Data.TargetName))
            {
                return label;
            }
            else
            {
                return $"{label} ({message.Data.TargetName})";
            }
        }

        protected override async Task ProcessMessageAsync(T message, MessageReceivedProperties properties, CancellationToken cancellationToken)
        {
            var ci = LocalizationUtils.SetCulture(message.Data.Culture, true);
            Properties.Resources.Culture = ci;

            var title = GetLogTitle(message);

            await _logDb.LogAsync(message, title, cancellationToken);
        }
    }
}
