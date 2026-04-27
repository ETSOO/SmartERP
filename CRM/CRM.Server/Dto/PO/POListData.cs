namespace CRM.Server.Dto.PO
{
    /// <summary>
    /// PO list data
    /// 订单列表数据
    /// </summary>
    public record POListData
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
    }
}

