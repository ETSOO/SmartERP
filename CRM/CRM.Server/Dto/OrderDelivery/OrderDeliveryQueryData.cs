using PlatformShared.Database.Models;

namespace CRM.Server.Dto.OrderDelivery
{
    /// <summary>
    /// Order delivery query data
    /// 订单配送方式查询数据
    /// </summary>
    public record OrderDeliveryQueryData
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
        public OrderDeliveryKind Kind { get; init; }

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
        /// Is valid
        /// 是否有效
        /// </summary>
        public bool IsValid { get; init; }
    }
}
