using com.etsoo.CoreFramework.Business;
using PlatformShared.Dto;

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
        /// Related product id
        /// 关联的产品编号
        /// </summary>
        public int? ProductId { get; set; }

        /// <summary>
        /// Related product category id
        /// 关联的产品类目编号
        /// </summary>
        public int? ProductCategoryId { get; set; }

        /// <summary>
        /// Related person (customer) id
        /// 关联的人员（客户）编号
        /// </summary>
        public long? PersonId { get; set; }

        /// <summary>
        /// Related person category id
        /// 关联的人员类目编号
        /// </summary>
        public int? PersonCategoryId { get; set; }

        /// <summary>
        /// Promotion code
        /// 促销码
        /// </summary>
        public PromotionCode Code { get; set; } = default!;

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
        public int Coupons { get; set; }

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
