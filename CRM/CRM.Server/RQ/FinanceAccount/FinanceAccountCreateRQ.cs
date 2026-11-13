using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Business;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;
using com.etsoo.WebUtils.Attributes;
using PlatformShared.Database.Models;

namespace CRM.Server.RQ.FinanceAccount
{
    /// <summary>
    /// Finance account create request data
    /// 财务账户创建请求数据
    /// </summary>
    public record FinanceAccountCreateRQ : IModelValidator
    {
        /// <summary>
        /// Owern person Id
        /// 所有人人员编号
        /// </summary>
        public long PersonId { get; init; }

        /// <summary>
        /// Kind
        /// 类型
        /// </summary>
        public FinanceAccountKind Kind { get; init; }

        /// <summary>
        /// Bank name
        /// 银行名称
        /// </summary>
        public required string Bank { get; init; }

        /// <summary>
        /// Currency
        /// 币种
        /// </summary>
        public required string Currency { get; init; }

        /// <summary>
        /// Account number
        /// 账号
        /// </summary>
        public required string AccountNumber { get; init; }

        /// <summary>
        /// SWIFT data
        /// SWIFT 数据
        /// </summary>
        public string? Swift { get; init; }

        /// <summary>
        /// Description
        /// 描述
        /// </summary>
        public string? Description { get; init; }

        /// <summary>
        /// Status
        /// 状态
        /// </summary>
        public EntityStatus? Status { get; init; }

        /// <summary>
        /// Expiry
        /// 过期时间
        /// </summary>
        public DateTimeOffset? Expiry { get; init; }

        /// <summary>
        /// Product Id
        /// 产品编号
        /// </summary>
        public int? ProductId { get; init; }

        /// <summary>
        /// Validate the model
        /// 验证模块
        /// </summary>
        /// <returns>Result</returns>
        public IActionResult? Validate()
        {
            if (Bank.Length is not (>= 1 and <= 128))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Bank));
            }

            if (!new CurrencyAttribute().IsValid(Currency))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Currency));
            }

            if (Description != null && Description.Length is not (>= 1 and <= 128))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Description));
            }

            return null;
        }
    }
}
