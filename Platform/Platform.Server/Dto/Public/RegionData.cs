namespace Platform.Server.Dto.Public
{
    /// <summary>
    /// Region data
    /// 地区数据
    /// </summary>
    public record RegionData
    {
        /// <summary>
        /// Id, like CN
        /// 编号，如CN
        /// </summary>
        public required string Id { get; init; }

        /// <summary>
        /// Id with 3 characters, like CHN
        /// 3字符编号
        /// </summary>
        public required string Id3 { get; init; }

        /// <summary>
        /// Name, like China
        /// 名称，如中国
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// English name, like China
        /// 英文名
        /// </summary>
        public required string EnglishName { get; init; }

        /// <summary>
        /// Currency id
        /// 货币编号
        /// </summary>
        public required string Currency { get; init; }

        /// <summary>
        /// Cultures supported
        /// 支持的文化
        /// </summary>
        public required IEnumerable<string> Cultures { get; init; }
    }
}
