using PlatformShared.Database.Models;

namespace CRM.Server.Dto.Order
{
    /// <summary>
    /// Order duplicate test data
    /// 订单重复测试数据
    /// </summary>
    public record OrderDuplicateTestData
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public long Id { get; init; }

        /// <summary>
        /// Title
        /// 标题
        /// </summary>
        public required string Title { get; init; }

        /// <summary>
        /// Kind
        /// 类型
        /// </summary>
        public OrderKind Kind { get; init; }
    }
}
