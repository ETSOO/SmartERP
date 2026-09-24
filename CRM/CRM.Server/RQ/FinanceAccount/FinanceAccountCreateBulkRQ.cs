using com.etsoo.CoreFramework.Application;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;
using com.etsoo.WebUtils.Attributes;
using PlatformShared.Database.Models;

namespace CRM.Server.RQ.FinanceAccount
{
    /// <summary>
    /// Finance account create bulk request data
    /// 批量创建财务账户请求数据
    /// </summary>
    public record FinanceAccountCreateBulkRQ : IModelValidator
    {
        /// <summary>
        /// Owner person id
        /// 所有人人员编号
        /// </summary>
        public long? PersonId { get; init; }

        /// <summary>
        /// Kind
        /// 类型
        /// </summary>
        public FinanceAccountKind Kind { get; init; }

        /// <summary>
        /// Bank
        /// 银行
        /// </summary>
        public required string Bank { get; init; }

        /// <summary>
        /// Currency
        /// 币种
        /// </summary>
        public required string Currency { get; init; }

        /// <summary>
        /// Account prefix
        /// 账号前缀
        /// </summary>
        public required string Prefix { get; init; }

        /// <summary>
        /// Length
        /// 长度
        /// </summary>
        public required byte Length { get; init; }

        /// <summary>
        /// Start number
        /// 开始数字
        /// </summary>
        public required int StartNumber { get; init; }

        /// <summary>
        /// Count
        /// 数量
        /// </summary>
        public required int Count { get; init; }

        /// <summary>
        /// Balance, ignore when equal to 0
        /// 余额，等于 0 时忽略
        /// </summary>
        public required decimal Balance { get; init; }

        /// <summary>
        /// Product id
        /// 产品编号
        /// </summary>
        public int? ProductId { get; init; }

        /// <summary>
        /// Description
        /// 描述
        /// </summary>
        public string? Description { get; init; }

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

            if (Kind == FinanceAccountKind.Pass)
            {
                if (ProductId == null || ProductId <= 0)
                {
                    return ApplicationErrors.NoValidData.AsResult(nameof(ProductId));
                }
            }

            if (Length < 4)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Length));
            }

            if (Prefix.Length + 1 >= Length)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Prefix));
            }

            if (StartNumber < 1)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(StartNumber));
            }

            if (Count < 2 || Count > 5000)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Count));
            }

            if (Description != null && Description.Length is not (>= 1 and <= 128))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Description));
            }

            return null;
        }
    }
}
