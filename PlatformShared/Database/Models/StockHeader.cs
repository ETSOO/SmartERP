using PlatformShared.Dto;

namespace PlatformShared.Database.Models
{
    /// <summary>
    /// Stock header
    /// 库存
    /// </summary>
    public class StockHeader
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Organization id
        /// 所属机构
        /// </summary>
        public int OrganizationId { get; set; }

        /// <summary>
        /// Kind
        /// 类型
        /// </summary>
        public StockKind Kind { get; set; }

        /// <summary>
        /// Customer or supplier ID
        /// 发货时是客户编号，入库时是供应商编号
        /// </summary>
        public long PersonId { get; set; }

        /// <summary>
        /// Shipping address id
        /// 发货地址编号
        /// </summary>
        public int LocationFromId { get; set; }

        /// <summary>
        /// Receiving address id
        /// 收货地址编号
        /// </summary>
        public int LocationToId { get; set; }

        /// <summary>
        /// User id
        /// 操作用户编号
        /// </summary>
        public long UserId { get; set; }

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
        /// Tracking number
        /// 物流编号
        /// </summary>
        public string? TrackingNumber { get; set; }

        /// <summary>
        /// Order / PO ids
        /// 相关订单 / 采购 编号
        /// </summary>
        public List<long>? OrderIds { get; set; }

        /// <summary>
        /// Receipt time, null means in transit
        /// 收货时间，空表示在途
        /// </summary>
        public DateTimeOffset? ReceiptTime { get; set; }

        /// <summary>
        /// Creation
        /// 登记时间
        /// </summary>
        public DateTimeOffset Creation { get; set; }

        /// <summary>
        /// Total lines
        /// 总行数
        /// </summary>
        public int TotalLines { get; set; }

        /// <summary>
        /// Total qty
        /// 总数量
        /// </summary>
        public decimal TotalQty { get; set; }

        /// <summary>
        /// Shipping location
        /// 发货位置
        /// </summary>
        public PersonAddress LocationFrom { get; set; } = default!;

        /// <summary>
        /// Receiving location
        /// 收货位置
        /// </summary>
        public PersonAddress LocationTo { get; set; } = default!;

        /// <summary>
        /// Customer or supplier
        /// 客户或供应商
        /// </summary>
        public Person Person { get; set; } = default!;

        /// <summary>
        /// User
        /// 用户
        /// </summary>
        public Person User { get; set; } = default!;

        /// <summary>
        /// Stock lines
        /// 库存行
        /// </summary>
        public ICollection<StockLine> Lines { get; set; } = default!;
    }
}
