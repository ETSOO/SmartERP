using com.etsoo.ThirdPartyExtentions.Minio;
using com.etsoo.Utils.Storage;
using Minio;
using PlatformShared.Services;
using System.Text.RegularExpressions;

namespace Platform.Server.Services
{
    /// <summary>
    /// Storage factory
    /// 存储工厂
    /// </summary>
    public partial class StorageFactory : IStorageFactory
    {
        readonly ISmartERPCoordinator _erp;
        readonly IStorage _storage;
        readonly IMinioClientFactory _factory;

        public StorageFactory(ISmartERPCoordinator erp, IStorage storage, IMinioClientFactory factory)
        {
            _erp = erp;
            _storage = storage;
            _factory = factory;
        }

        /// <summary>
        /// Create storage instance
        /// 创建存储实例
        /// </summary>
        /// <param name="orgId">Organization ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async ValueTask<IStorage> CreateAsync(int? orgId, CancellationToken cancellationToken = default)
        {
            if (orgId > 0)
            {
                var item = await _erp.GetStorageApiAsync(orgId.Value, cancellationToken);
                if (item != null)
                {
                    var appSecret = _erp.DecriptData(item.AppSecret, ServiceConstants.CoreApiAppSecretEncryptionKey);

                    var options = new S3StorageOptions
                    {
                        AccessKey = item.AppId,
                        SecretKey = appSecret,
                        Endpoint = item.Endpoint.ToString(),

                        Root = item.Options.Root,
                        URLRoot = item.Options.UrlRoot.ToString()
                    };

                    return new S3Storage(_factory, options, true);
                }
            }

            return _storage;
        }

        /// <summary>
        /// Get organization path
        /// 获取机构路径
        /// </summary>
        /// <param name="orgId">Organization ID</param>
        /// <param name="folder">Folder name</param>
        /// <returns>Result</returns>
        public string GetOrgPath(int orgId, string folder)
        {
            var subLevel = (orgId / 20000).ToString().PadLeft(5, '0');
            return $"/Orgs/L{subLevel}/Org{orgId}/{folder}/{DateTime.UtcNow:yyyy}/";
        }

        /// <summary>
        /// Get organization ID from path
        /// 从路径中获取机构编号
        /// </summary>
        /// <param name="path">Organization path</param>
        /// <returns>Result</returns>
        public int GetOrgIdFromPath(string path)
        {
            // Match the pattern L*****/Org{orgId}/
            if (path.StartsWith('L'))
            {
                var match = MyRegex().Match(path);
                if (match.Success && match.Groups.Count > 1)
                {
                    if (int.TryParse(match.Groups[1].Value, out int orgId))
                        return orgId;
                }
            }

            return 0;
        }

        [GeneratedRegex(@"/Org(\d+)/")]
        private static partial Regex MyRegex();
    }
}
