using com.etsoo.MessageQueue;
using System.Text.Json.Serialization.Metadata;

namespace PlatformShared.Extentions
{
    public interface IQueueService
    {
        Task PushAsync<T>(T message, JsonTypeInfo<T> typeInfo, CancellationToken cancellationToken = default) where T : IMessageQueueMessage;
    }
}