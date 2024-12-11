namespace Platform.Server.Dto.Org
{
    /// <summary>
    /// Get user's latest accessed organizations data
    /// 获取用户最近访问的机构数据
    /// </summary>
    public record OrgGetMyData
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Name
        /// 名称
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// Brand
        /// 品牌
        /// </summary>
        public string? Brand { get; init; }
    }
}
