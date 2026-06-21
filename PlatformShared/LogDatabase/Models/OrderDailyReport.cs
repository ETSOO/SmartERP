namespace PlatformShared.LogDatabase.Models
{
    /// <summary>
    /// Order daily report
    /// 订单日报表
    /// </summary>
    public class OrderDailyReport
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Organization id
        /// 机构编号
        /// </summary>
        public int OrganizationId { get; set; }

        /// <summary>
        /// Period
        /// 区间
        /// </summary>
        public DateOnly Period { get; set; }

        /// <summary>
        /// Order Items
        /// 订单项目
        /// </summary>
        public int Items { get; set; }

        /// <summary>
        /// Amount
        /// 金额
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Paid amount
        /// 已付款金额
        /// </summary>
        public decimal PaidAmount { get; set; }

        /// <summary>
        /// Discount
        /// 折扣
        /// </summary>
        public decimal Discount { get; set; }

        /// <summary>
        /// Line discount
        /// 行折扣
        /// </summary>
        public decimal LineDiscount { get; set; }

        /// <summary>
        /// Approved discount
        /// 授权折扣
        /// </summary>
        public decimal ApprovedDiscount { get; set; }

        /// <summary>
        /// Qty
        /// 数量
        /// </summary>
        public decimal Qty { get; set; }

        /// <summary>
        /// Customers
        /// 订单
        /// </summary>
        public int Customers { get; set; }
    }
}
