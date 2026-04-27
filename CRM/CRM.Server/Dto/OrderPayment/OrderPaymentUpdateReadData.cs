using PlatformShared.Database.Models;

namespace CRM.Server.Dto.OrderPayment
{
    /// <summary>
    /// Order payment update read data
    /// 更新订单支付方式读取数据
    /// </summary>
    public record OrderPaymentUpdateReadData
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
        public OrderPaymentKind Kind { get; init; }

        /// <summary>
        /// Title
        /// 标题
        /// </summary>
        public required string Title { get; init; }

        /// <summary>
        /// Is order or not
        /// 是否为订单
        /// </summary>
        public bool IsOrder { get; init; }

        /// <summary>
        /// Is valid
        /// 是否有效
        /// </summary>
        public bool IsValid { get; init; }

        /// <summary>
        /// Order index
        /// 排序数
        /// </summary>
        public short OrderIndex { get; init; }
    }
}
