using com.etsoo.CoreFramework.Models;
using com.etsoo.Utils.Actions;
using PlatformShared.Database.Models;
using PlatformShared.Dto;
using PlatformShared.Services.ApiOptions;
using System.Text.Json.Serialization.Metadata;

namespace PlatformShared.Services
{
    public interface ISmartERPCoordinator
    {
        string DecriptData(string cipherText, string key = "");
        Task<AppData?> GetAppSecretAsync(int appId, string appKey, CancellationToken cancellationToken = default);
        Task<ApiItem?> GetApiAsync(int orgId, CoreApiService service, CancellationToken cancellationToken = default);
        Task<ApiItem<T>?> GetApiAsync<T>(int orgId, CoreApiService service, JsonTypeInfo<T> typeInfo, CancellationToken cancellationToken = default) where T : class;
        Task<ApiItem<SMTPApiOptions>?> GetSMTPApiAsync(int orgId, CancellationToken cancellationToken = default);
        Task<ApiItem<StorageApiOptions>?> GetStorageApiAsync(int orgId, CancellationToken cancellationToken = default);
        ValueTask<ActionResult> ValidateActionAsync(string jsonSign, string action, long targetId, CancellationToken cancellationToken = default);
        Task<ActionResult> ValidateActionAsync(AppActionData data, CancellationToken cancellationToken = default);
    }
}