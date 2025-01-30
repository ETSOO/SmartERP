using com.etsoo.DI;
using com.etsoo.MessageQueue;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization.Metadata;

namespace PlatformShared.Extentions
{
    /// <summary>
    /// Queue service
    /// 队列服务
    /// </summary>
    public class QueueService : IQueueService
    {
        private readonly IFireAndForgetService _fireAndForget;
        private readonly IMessageQueueProducer _queueProducer;

        public QueueService(IFireAndForgetService fireAndForget, IMessageQueueProducer queueProducer)
        {
            _fireAndForget = fireAndForget;
            _queueProducer = queueProducer;
        }

        /// <summary>
        /// Push message to queue
        /// 推送消息到队列
        /// </summary>
        /// <typeparam name="T">Generic data type</typeparam>
        /// <param name="message">Message</param>
        /// <param name="typeInfo">JSON type info</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public Task PushAsync<T>(T message, JsonTypeInfo<T> typeInfo, CancellationToken cancellationToken = default) where T : IMessageQueueMessage
        {
            // Fire and forget
            _fireAndForget.FireAsync(async (logger) =>
            {
                try
                {
                    await _queueProducer.SendJsonAsync(message, typeInfo, T.Type);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Push message failed with {@message}", message);
                }
            });

            return Task.CompletedTask;
        }
    }
}
