namespace PlatformShared.Dto.Document.Order
{
    /// <summary>
    /// Order line data
    /// 订单行数据
    /// </summary>
    public record OrderLineData
    {
        /// <summary>
        /// Modifiers
        /// 扩展属性
        /// </summary>
        public Dictionary<string, object>? Modifiers { get; init; }
    }
}
