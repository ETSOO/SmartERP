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
        /// Product name
        /// 产品名称
        /// </summary>
        public required string Product { get; init; }

        /// <summary>
        /// Title
        /// 标题
        /// </summary>
        public required string Sn { get; init; }

        /// <summary>
        /// Expiry
        /// 到期时间
        /// </summary>
        public DateTimeOffset Expiry { get; init; }
    }
}
