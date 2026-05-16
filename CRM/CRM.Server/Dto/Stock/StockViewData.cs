using com.etsoo.CoreFramework.Models;
using com.etsoo.Utils.Models;
using PlatformShared.Dto;

namespace CRM.Server.Dto.Stock
{
    /// <summary>
    /// Stock view data
    /// 库存浏览数据
    /// </summary>
    public record StockViewData
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public long Id { get; init; }

        /// <summary>
        /// Kind
        /// 类型
        /// </summary>
        public StockKind Kind { get; init; }

        /// <summary>
        /// Shipping address id
        /// 发货地址编号
        /// </summary>
        public int LocationFromId { get; init; }

        /// <summary>
        /// Shipping address
        /// 发货地址
        /// </summary>
        public required string LocationFrom { get; init; }

        /// <summary>
        /// Receiving address id
        /// 收货地址编号
        /// </summary>
        public int LocationToId { get; init; }

        /// <summary>
        /// Receiving address
        /// 收货地址
        /// </summary>
        public required string LocationTo { get; init; }

        /// <summary>
        /// Title
        /// 标题
        /// </summary>
        public required string Title { get; init; }

        /// <summary>
        /// Description
        /// 描述
        /// </summary>
        public string? Description { get; init; }

        /// <summary>
        /// Customer or supplier ID
        /// 发货时是客户编号，入库时是供应商编号
        /// </summary>
        public long PersonId { get; init; }

        /// <summary>
        /// Name
        /// 名称
        /// </summary>
        public required string PersonName { get; init; }

        /// <summary>
        /// User id
        /// 操作用户编号
        /// </summary>
        public long UserId { get; init; }

        /// <summary>
        /// User name
        /// 操作用户姓名
        /// </summary>
        public required string UserName { get; init; }

        /// <summary>
        /// Orders
        /// 订单
        /// </summary>
        public IEnumerable<IdLabelItem<long>>? Orders { get; init; }

        /// <summary>
        /// Tracking number
        /// 物流编号
        /// </summary>
        public string? TrackingNumber { get; init; }

        /// <summary>
        /// Total lines
        /// 总行数
        /// </summary>
        public int TotalLines { get; init; }

        /// <summary>
        /// Total qty
        /// 总数量
        /// </summary>
        public decimal TotalQty { get; init; }

        /// <summary>
        /// Receipt time, null means in transit
        /// 收货时间，空表示在途
        /// </summary>
        public DateTimeOffset? ReceiptTime { get; init; }

        /// <summary>
        /// Creation
        /// 登记时间
        /// </summary>
        public DateTimeOffset Creation { get; init; }

        /// <summary>
        /// Is deletable
        /// 是否可删除
        /// </summary>
        public bool IsDeletable { get; set; }
    }
}
