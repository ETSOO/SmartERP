using com.etsoo.CoreFramework.Business;

namespace PlatformShared.Database.Models
{
    /// <summary>
    /// Promotion
    /// 促销
    /// </summary>
    public class Promotion
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Core organization id
        /// 核心机构编号
        /// </summary>
        public int CoreOrganizationId { get; set; }

        /// <summary>
        /// Title
        /// 标题
        /// </summary>
        public string Title { get; set; } = default!;

        /// <summary>
        /// Currency
        /// 币种
        /// </summary>
        public string Currency { get; set; } = default!;

        /// <summary>
        /// Related product ids
        /// 关联的产品编号
        /// </summary>
        public List<int>? ProductIds { get; set; }

        /// <summary>
        /// Related product category ids
        /// 关联的产品类目编号
        /// </summary>
        public List<int>? ProductCategoryIds { get; set; }

        /// <summary>
        /// Related person (customer) ids
        /// 关联的人员（客户）编号
        /// </summary>
        public List<long>? PersonIds { get; set; }

        /// <summary>
        /// Related person category ids
        /// 关联的人员类目编号
        /// </summary>
        public List<int>? PersonCategoryIds { get; set; }

        /// <summary>
        /// Promotion code
        /// 促销码
        /// </summary>
        public short Code { get; set; } = default!;

        /// <summary>
        /// Minimum spending amount
        /// 最低消费金额
        /// </summary>
        public decimal MinAmount { get; set; }

        /// <summary>
        /// Discount percentage, like 10 for 10%
        /// 折扣百分比，如 10 代表 10%
        /// </summary>
        public int Discount { get; set; }

        /// <summary>
        /// Valid start date
        /// 有效开始日期
        /// </summary>
        public DateTimeOffset ValidStart { get; set; }

        /// <summary>
        /// Valid end date
        /// 有效结束日期
        /// </summary>
        public DateTimeOffset ValidEnd { get; set; }

        /// <summary>
        /// Number of coupons
        /// 优惠券数量
        /// </summary>
        public int? Coupons { get; set; }

        /// <summary>
        /// Number of coupons applied
        /// 已使用优惠券数量
        /// </summary>
        public int CouponsApplied {  get; set; }

        /// <summary>
        /// Whether the promotion is stackable
        /// 优惠是否可叠加
        /// </summary>
        public bool Stackable { get; set; }

        /// <summary>
        /// Order index
        /// 排序数
        /// </summary>
        public short OrderIndex { get; set; }

        /// <summary>
        /// Creation
        /// 登记时间
        /// </summary>
        public DateTimeOffset Creation { get; set; }

        /// <summary>
        /// Status
        /// 状态
        /// </summary>
        public EntityStatus Status { get; set; }
    }
}
