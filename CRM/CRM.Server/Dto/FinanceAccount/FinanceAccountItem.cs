using PlatformShared.Database.Models;

namespace CRM.Server.Dto.FinanceAccount
{
    /// <summary>
    /// Finance account item
    /// 财务账号项目
    /// </summary>
    public record FinanceAccountItem
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public int Id { get; init; }

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
        /// Account number
        /// 账号
        /// </summary>
        public required string AccountNumber { get; init; }

        /// <summary>
        /// Product ID
        /// 产品编号
        /// </summary>
        public int? ProductId { get; init; }
    }

    /// <summary>
    /// Finance account item
    /// 财务账号项目
    /// </summary>
    public record FinanceAccountItemWithBalance : FinanceAccountItem
    {
        /// <summary>
        /// Balance
        /// 余额
        /// </summary>
        public decimal Balance { get; init; }
    }
}
