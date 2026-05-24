namespace Admin.Server.Dto.Document
{
    /// <summary>
    /// Document query data
    /// 文档查询数据
    /// </summary>
    public record DocumentQueryData
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

        /// <summary>
        /// Has parameters or not
        /// 是否有参数
        /// </summary>
        public bool HasParameters { get; init; }

        /// <summary>
        /// Refresh time
        /// 刷新时间
        /// </summary>
        public DateTimeOffset RefreshTime { get; init; }
    }
}
