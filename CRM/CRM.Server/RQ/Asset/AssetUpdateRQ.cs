using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Business;
using com.etsoo.CoreFramework.Models;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;
using com.etsoo.Utils.String;

namespace CRM.Server.RQ.Asset
{
    /// <summary>
    /// Update asset request data
    /// 更新资产请求数据
    /// </summary>
    public record AssetUpdateRQ : UpdateModel<int>, IModelValidator
    {
        /// <summary>
        /// Person (owner) id
        /// 所有者编号
        /// </summary>
        public long? PersonId { get; init; }

        /// <summary>
        /// Product id
        /// 产品编号
        /// </summary>
        public int? ProductId { get; init; }

        /// <summary>
        /// Supplier id
        /// 供应商编号
        /// </summary>
        public long? SupplierId { get; init; }

        /// <summary>
        /// Serial number
        /// 序列号
        /// </summary>
        public string? Sn { get; init; }

        /// <summary>
        /// Description
        /// 描述
        /// </summary>
        public string? Description { get; init; }

        /// <summary>
        /// Expiry
        /// 到期时间
        /// </summary>
        public DateTimeOffset? Expiry { get; init; }

        /// <summary>
        /// Remaining times
        /// 剩余次数
        /// </summary>
        public short? Times { get; init; }

        /// <summary>
        /// Remaining amount
        /// 剩余金额
        /// </summary>
        public decimal? Amount { get; init; }

        /// <summary>
        /// Sensitive data
        /// 敏感数据
        /// </summary>
        public string? SensitiveData { get; init; }

        /// <summary>
        /// Health check URL
        /// 健康检查网址
        /// </summary>
        public string? HealthCheckUrl { get; init; }

        /// <summary>
        /// JSON data
        /// JSON 数据
        /// </summary>
        public string? Data { get; init; }

        /// <summary>
        /// Status
        /// 状态
        /// </summary>
        public EntityStatus? Status { get; init; }

        /// <summary>
        /// Validate the model
        /// 验证模块
        /// </summary>
        /// <returns>Result</returns>
        public IActionResult? Validate()
        {
            if (Sn != null && Sn.Length is not (>= 1 and <= 256))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Sn));
            }

            if (Description != null && Description.Length is not (>= 1 and <= 1280))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Description));
            }

            if (SensitiveData != null && SensitiveData.Length is not (>= 1 and <= 128))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(SensitiveData));
            }

            if (HealthCheckUrl != null && HealthCheckUrl.Length is not (>= 1 and <= 1280))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(HealthCheckUrl));
            }

            if (Data != null && !Data.IsJson())
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Data));
            }

            return null;
        }
    }
}
