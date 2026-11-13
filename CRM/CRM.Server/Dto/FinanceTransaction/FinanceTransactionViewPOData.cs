using CRM.Server.Dto.FinanceAccount;
using CRM.Server.Dto.Product;

namespace CRM.Server.Dto.FinanceTransaction
{
    /// <summary>
    /// Purchase order main data
    /// 采购订单主数据
    /// </summary>
    public record FinanceTransactionPOViewMainData
    {
        /// <summary>
        /// Order ID
        /// 订单编号
        /// </summary>
        public long Id { get; init; }

        /// <summary>
        /// Supplier id
        /// 供应商编号
        /// </summary>
        public long SupplierId { get; init; }

        /// <summary>
        /// Supplier name
        /// 供应商姓名
        /// </summary>
        public required string SupplierName { get; init; }

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
    /// Finance transaction view purchase order data
    /// 财务交易浏览采购订单数据
    /// </summary>
    public record FinanceTransactionViewPOData
    {
        /// <summary>
        /// Purchase order
        /// 采购订单
        /// </summary>
        public required FinanceTransactionPOViewMainData Order { get; init; }

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
