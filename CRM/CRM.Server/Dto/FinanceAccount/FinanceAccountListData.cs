using PlatformShared.Database.Models;

namespace CRM.Server.Dto.FinanceAccount
{
    /// <summary>
    /// Finance account list data
    /// 财务账户列表数据
    /// </summary>
    public record FinanceAccountListData
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
        /// Currency
        /// 币种
        /// </summary>
        public required string Currency { get; init; }

        /// <summary>
        /// Account number
        /// 账号
        /// </summary>
        public required string AccountNumber { get; init; }
    }
}
