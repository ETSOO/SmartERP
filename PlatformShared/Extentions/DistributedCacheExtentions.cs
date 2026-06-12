using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace com.etsoo.Utils.Serialization
{
    /// <summary>
    /// Distributed cache extensions
    /// 分布式缓存扩展
    /// </summary>
    public static class DistributedCacheExtentions
    {
        /// <summary>
        /// Synchronously gets the value associated with this key if it exists, or generates a new entry using the provided key and a value from the given factory if the key is not found.
        /// </summary>
        /// <typeparam name="T">The type of the object to get.</typeparam>
        /// <param name="cache">The <see cref="IDistributedCache"/> instance this method extends.</param>
        /// <param name="key">A string identifying the requested value.</param>
        /// <param name="factory">The factory task that creates the value associated with this key if the key does not exist in the cache.</param>
        /// <param name="typeInfo">The JSON type information for the object to get.</param>
        /// <returns>Result</returns>
        public static T? GetOrCreate<T>(this IDistributedCache cache, string key, Func<DistributedCacheEntryOptions, T?> factory, JsonTypeInfo<T> typeInfo)
        {
            var bytes = cache.Get(key);
            if (bytes != null)
            {
                return JsonSerializer.Deserialize(bytes, typeInfo);
            }

            var options = new DistributedCacheEntryOptions();
            var item = factory(options);

            if (item == null)
                return default;

            var newBytes = JsonSerializer.SerializeToUtf8Bytes(item, typeInfo);

            cache.Set(key, newBytes, options);

            return item;
        }

        /// <summary>
        /// Asynchronously gets the value associated with this key if it exists, or generates a new entry using the provided key and a value from the given factory if the key is not found.
        /// </summary>
        /// <typeparam name="T">The type of the object to get.</typeparam>
        /// <param name="cache">The <see cref="IDistributedCache"/> instance this method extends.</param>
        /// <param name="key">A string identifying the requested value.</param>
        /// <param name="factory">The factory task that creates the value associated with this key if the key does not exist in the cache.</param>
        /// <param name="typeInfo">The JSON type information for the object to get.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public static async ValueTask<T?> GetOrCreateAsync<T>(this IDistributedCache cache, string key, Func<DistributedCacheEntryOptions, Task<T?>> factory, JsonTypeInfo<T> typeInfo, CancellationToken cancellationToken = default)
        {
            // 避免 await 之后回到原上下文，从而减少线程切换和调度开销
            var bytes = await cache.GetAsync(key, cancellationToken).ConfigureAwait(false);

            if (bytes != null)
            {
                return JsonSerializer.Deserialize(bytes, typeInfo);
            }

            var options = new DistributedCacheEntryOptions();

            var item = await factory(options).ConfigureAwait(false);
            if (item == null)
                return default;

            var newBytes = JsonSerializer.SerializeToUtf8Bytes(item, typeInfo);

            await cache.SetAsync(key, newBytes, options, cancellationToken)
                       .ConfigureAwait(false);

            return item;
        }
    }
}
