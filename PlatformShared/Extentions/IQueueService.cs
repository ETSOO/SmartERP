using com.etsoo.MessageQueue;
using com.etsoo.Utils.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace PlatformShared.Extentions
{
    public interface IQueueService
    {
        Task FirePushAsync<T>(T message, JsonTypeInfo<T> typeInfo, CancellationToken cancellationToken = default) where T : IMessageQueueMessage;
        Task<string> PushAsync<T>(T message, JsonTypeInfo<T> typeInfo, CancellationToken cancellationToken = default) where T : IMessageQueueMessage;
        Task<string> PushAsync<T>(T message, JsonTypeInfo<T> typeInfo, MessageProperties properties, CancellationToken cancellationToken = default) where T : IMessageQueueMessage;
    }
}