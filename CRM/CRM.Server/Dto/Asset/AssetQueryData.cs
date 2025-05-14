namespace CRM.Server.Dto.Asset
{
    /// <summary>
    /// Asset query data
    /// 资产查询数据
    /// </summary>
    public record AssetQueryData
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
