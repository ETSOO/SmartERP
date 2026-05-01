using com.etsoo.CoreFramework.Business;
using System.Text.Json;

namespace CRM.Server.Dto.POLine
{
    /// <summary>
    /// Purchase line update read data
    /// 更新采购行读取数据
    /// </summary>
    public record POLineUpdateReadData
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public long Id { get; init; }

        /// <summary>
        /// Purchase line id
        /// 采购行编号
        /// </summary>
        public long POId { get; init; }

        /// <summary>
        /// Currency
        /// 币种
        /// </summary>
        public required string Currency { get; init; }

        /// <summary>
        /// Product id
        /// 产品编号
        /// </summary>
        public int ProductId { get; init; }

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
        /// Original price
        /// 原价
        /// </summary>
        public decimal OriginalPrice { get; init; }

        /// <summary>
        /// Cost price
        /// 成本价
        /// </summary>
        public decimal CostPrice { get; init; }

        /// <summary>
        /// Price
        /// 价格
        /// </summary>
        public decimal Price { get; init; }

        /// <summary>
        /// Qty
        /// 数量
        /// </summary>
        public decimal Qty { get; init; }

        /// <summary>
        /// Start time
        /// 开始时间
        /// </summary>
        public DateTimeOffset? StartTime { get; init; }

        /// <summary>
        /// End time
        /// 结束时间
        /// </summary>
        public DateTimeOffset? EndTime { get; init; }

        /// <summary>
        /// Supplier id
        /// 供应商编号
        /// </summary>
        public long? SupplierId { get; init; }

        /// <summary>
        /// User id
        /// 用户编号
        /// </summary>
        public long? UserId { get; init; }

        /// <summary>
        /// Modifiers
        /// 定制选项
        /// </summary>
        public JsonDocument? Modifiers { get; init; }

        /// <summary>
        /// JSON data
        /// JSON 数据
        /// </summary>
        public JsonDocument? Data { get; init; }

        /// <summary>
        /// Status
        /// 状态
        /// </summary>
        public EntityStatus Status { get; init; }

        /// <summary>
        /// Is deletable
        /// 是否可删除
        /// </summary>
        public bool IsDeletable { get; init; }
    }
}
