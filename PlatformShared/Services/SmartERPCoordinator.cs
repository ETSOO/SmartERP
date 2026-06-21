using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Models;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Crypto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using PlatformShared.Dto;
using PlatformShared.Services.ApiOptions;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace PlatformShared.Services
{
    /// <summary>
    /// SmartERP coordinator service
    /// 司友云ERP协调服务
    /// </summary>
    public class SmartERPCoordinator : ISmartERPCoordinator
    {
        private readonly IDbContextFactory<MyDbContext> _dbFactory;
        private readonly SmartERPCoordinatorOptions _options;

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="dbFactory">Database context factory</param>
        /// <param name="options">Options</param>
        public SmartERPCoordinator(IDbContextFactory<MyDbContext> dbFactory,
            IOptions<SmartERPCoordinatorOptions> options)
        {
            _dbFactory = dbFactory;
            _options = options.Value;
        }

        /// <summary>
        /// Decript data
        /// 解密数据
        /// </summary>
        /// <param name="cipherText">Cipher text</param>
        /// <param name="key">Key</param>
        /// <returns>Result</returns>
        public string DecriptData(string cipherText, string key = "")
        {
            var bytes = CryptographyUtils.AESDecrypt(cipherText, key + _options.PrivateKey) ?? throw new ApplicationException("Decript Data Failed");
            return Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// Get app secret
        /// 获取应用程序密钥
        /// </summary>
        /// <param name="appId">App ID</param>
        /// <param name="appKey">App key</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<AppData?> GetAppSecretAsync(int appId, string appKey, CancellationToken cancellationToken = default)
        {
            AppData? data;

            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

            if (string.IsNullOrEmpty(appKey))
            {
                data = await db.CoreApps.AsNoTracking().Where(a => a.Id == appId).Select(a => new AppData { AppSecret = a.AppSecret, Urls = a.Urls }).FirstOrDefaultAsync(cancellationToken);
            }
            else
            {
                data = await db.CoreOrganizationApps.AsNoTracking().Where(oa => oa.CoreAppId == appId && oa.AppKey == appKey && oa.AppSecret != null).Select(oa => new AppData { AppSecret = oa.AppSecret!, Urls = oa.LocalUrls ?? oa.CoreApp.Urls }).FirstOrDefaultAsync(cancellationToken);
            }

            data?.AppSecret = DecriptData(data.AppSecret, "Token" + appId);

            return data;
        }

        /// <summary>
        /// Get API
        /// 获取接口
        /// </summary>
        /// <param name="orgId">Organization id</param>
        /// <param name="service">API service</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<ApiItem?> GetApiAsync(int orgId, CoreApiService service, CancellationToken cancellationToken = default)
        {
            var orgIdSP = new NpgsqlParameter<int>("p_org_id", orgId);
            var serviceSP = new NpgsqlParameter<short>("p_service", (short)service);

            // The returned columns naming should be the same as the model, otherwise EFCore.NamingConventions need to be used
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var data = (await db.Database.SqlQuery<ApiItem>($"SELECT * FROM get_core_api({orgIdSP}, {serviceSP})")
                .ToListAsync(cancellationToken)).FirstOrDefault();

            return data;
        }

        /// <summary>
        /// Get API
        /// 获取接口
        /// </summary>
        /// <typeparam name="T">Generic options type</typeparam>
        /// <param name="orgId">Organization id</param>
        /// <param name="service">API service</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<ApiItem<T>?> GetApiAsync<T>(int orgId, CoreApiService service, JsonTypeInfo<T> typeInfo, CancellationToken cancellationToken = default)
            where T : class
        {
            var item = await GetApiAsync(orgId, service, cancellationToken);
            if (item is null)
                return null;

            var options = JsonSerializer.Deserialize(item.JsonOptions ?? "{}", typeInfo)
                ?? throw new InvalidOperationException("Failed to deserialize JSON options");

            return new ApiItem<T>(item, options);
        }

        /// <summary>
        /// Get SMTP API options
        /// 获取邮件接口选项
        /// </summary>
        /// <param name="orgId">Organization id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public Task<ApiItem<SMTPApiOptions>?> GetSMTPApiAsync(int orgId, CancellationToken cancellationToken = default)
        {
            return GetApiAsync(orgId, CoreApiService.SMTP, PlatformSharedContext.Default.SMTPApiOptions, cancellationToken);
        }

        /// <summary>
        /// Get Storage API options
        /// 获取存储接口选项
        /// </summary>
        /// <param name="orgId">Organization id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public Task<ApiItem<StorageApiOptions>?> GetStorageApiAsync(int orgId, CancellationToken cancellationToken = default)
        {
            return GetApiAsync(orgId, CoreApiService.Storage, PlatformSharedContext.Default.StorageApiOptions, cancellationToken);
        }

        /// <summary>
        /// Validate action
        /// 验证操作
        /// </summary>
        /// <param name="jsonSign">Sign JSON string</param>
        /// <param name="action">Action name</param>
        /// <param name="targetId">Target id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async ValueTask<ActionResult> ValidateActionAsync(string jsonSign, string action, long targetId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(jsonSign))
            {
                return ApplicationErrors.NoValidData.AsResult("action");
            }

            var sign = JsonSerializer.Deserialize(jsonSign, ModelJsonSerializerContext.Default.AppActionData);
            if (sign is null)
            {
                return ApplicationErrors.NoValidData.AsResult("sign");
            }

            // Set action and target ID
            sign.Action = action;
            sign.TargetId = targetId;

            return await ValidateActionAsync(sign, cancellationToken);
        }

        /// <summary>
        /// Validate action
        /// 验证操作
        /// </summary>
        /// <param name="data">Action data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<ActionResult> ValidateActionAsync(AppActionData data, CancellationToken cancellationToken = default)
        {
            var appData = await GetAppSecretAsync(data.AppId, data.AppKey, cancellationToken);
            if (appData is null)
            {
                return ApplicationErrors.NoValidData.AsResult("app");
            }

            var expectedSign = data.SignWith(appData.AppSecret);
            if (!data.Sign.Equals(expectedSign))
            {
                return ApplicationErrors.NoValidData.AsResult("sign");
            }

            return ActionResult.Success;
        }
    }
}