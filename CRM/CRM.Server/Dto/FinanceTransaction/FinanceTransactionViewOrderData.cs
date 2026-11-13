using CRM.Server.Dto.FinanceAccount;
using CRM.Server.Dto.Product;

namespace CRM.Server.Dto.FinanceTransaction
{
    /// <summary>
    /// Order main data
    /// 订单主数据
    /// </summary>
    public record FinanceTransactionOrderViewMainData
    {
        /// <summary>
        /// Order ID
        /// 订单编号
        /// </summary>
        public long Id { get; init; }

        /// <summary>
        /// Customer id
        /// 客户编号
        /// </summary>
        public long CustomerId { get; init; }

        /// <summary>
        /// Customer name
        /// 客户姓名
        /// </summary>
        public required string CustomerName { get; init; }

        /// <summary>
        /// Title
        /// 标题
        /// </summary>
        public required string Title { get; init; }

        /// <summary>
        /// Payment method
        /// 付款方式
        /// </summary>
        public string? Payment { get; init; }

        /// <summary>
        /// Payment instruction
        /// 付款说明
        /// </summary>
        public string? PaymentInstruction { get; init; }

        /// <summary>
        /// Currency
        /// 币种
        /// </summary>
        public required string Currency { get; init; }

        /// <summary>
        /// Total payable
        /// 应付总额
        /// </summary>
        public decimal TotalPayable { get; init; }

        /// <summary>
        /// Paid amount
        /// 已付款金额
        /// </summary>
        public decimal PaidAmount { get; init; }

        /// <summary>
        /// Creation
        /// 登记时间
        /// </summary>
        public DateTimeOffset Creation { get; init; }

        /// <summary>
        /// Products
        /// 产品
        /// </summary>
        public required IEnumerable<ProductBomItem> Products { get; init; }
    }

    /// <summary>
    /// Finance transaction view order data
    /// 财务交易浏览订单数据
    /// </summary>
    public record FinanceTransactionViewOrderData
    {
        /// <summary>
        /// Order
        /// 订单
        /// </summary>
        public required FinanceTransactionOrderViewMainData Order { get; init; }

        /// <summary>
        /// Organization accounts
        /// 组织账户
        /// </summary>
        public required IEnumerable<FinanceAccountItem> OrgAccounts { get; init; }

        /// <summary>
        /// Accounts
        /// 账户
        /// </summary>
        public required IEnumerable<FinanceAccountItemWithBalance> Accounts { get; init; }
    }
}
