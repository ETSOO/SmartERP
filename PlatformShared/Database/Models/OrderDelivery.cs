namespace PlatformShared.Database.Models
{
    /// <summary>
    /// Order delivery kind
    /// 订单配送方式类型
    /// </summary>
    public enum OrderDeliveryKind : byte
    {
        /// <summary>
        /// Self pickup
        /// 自提
        /// </summary>
        Pickup = 1,

        /// <summary>
        /// Express
        /// 快递
        /// </summary>
        Express = 2,

        /// <summary>
        /// Freight
        /// 物流，适用于大件或大批量货物，成本较低但速度较慢
        /// </summary>
        Freight = 3,

        /// <summary>
        /// Sea freight
        /// 海运
        /// </summary>
        SeaFreight = 6,

        /// <summary>
        /// Air freight
        /// 空运
        /// </summary>
        AirFreight = 10,

        /// <summary>
        /// Rail freight
        /// 铁路货运
        /// </summary>
        RailFreight = 16,

        /// <summary>
        /// Online
        /// 在线交付
        /// </summary>
        Online = 99,

        /// <summary>
        /// Other
        /// 其他
        /// </summary>
        Other = 255
    }

    /// <summary>
    /// Order delivery
    /// 订单配送方式
    /// </summary>
    public class OrderDelivery
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Kind
        /// 类型
        /// </summary>
        public OrderDeliveryKind Kind { get; set; }

        /// <summary>
        /// Title
        /// 标题
        /// </summary>
        public string Title { get; set; } = default!;

        /// <summary>
        /// Description
        /// 描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Core organization id
        /// 核心机构编号
        /// </summary>
        public int CoreOrganizationId { get; set; }

        /// <summary>
        /// Is order or not
        /// 是否为订单
        /// </summary>
        public bool IsOrder { get; set; }

        /// <summary>
        /// Is valid
        /// 是否有效
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Order index
        /// 排序数
        /// </summary>
        public short OrderIndex { get; set; }

        /// <summary>
        /// Orders
        /// 订单
        /// </summary>
        public ICollection<OrderHeader> Orders { get; set; } = default!;
    }
}
