namespace CRM.Server.Dto.Asset
{
    /// <summary>
    /// Asset list data
    /// 资产列表数据
    /// </summary>
    public record AssetListData
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
