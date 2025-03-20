namespace PlatformShared.Dto
{
    /// <summary>
    /// Category item
    /// 类目项
    /// </summary>
    public record CategoryItem
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public required int Id { get; init; }

        /// <summary>
        /// Names
        /// 名称数组
        /// </summary>
        public required IEnumerable<string> Names { get; init; }
    }
}
