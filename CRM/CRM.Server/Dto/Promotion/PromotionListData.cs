namespace CRM.Server.Dto.Promotion
{
    /// <summary>
    /// Promotion list data
    /// 促销列表数据
    /// </summary>
    public record PromotionListData
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Title
        /// 标题
        /// </summary>
        public required string Title { get; init; }
    }
}
