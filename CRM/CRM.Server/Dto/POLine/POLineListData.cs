namespace CRM.Server.Dto.POLine
{
    /// <summary>
    /// Purchase line list data
    /// 采购行列表数据
    /// </summary>
    public record POLineListData
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
        /// Qty
        /// 数量
        /// </summary>
        public decimal Qty { get; init; }
    }
}
