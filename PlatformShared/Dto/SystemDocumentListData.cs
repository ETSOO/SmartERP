namespace PlatformShared.Dto
{
    /// <summary>
    /// System document list data
    /// 系统文档列表数据
    /// </summary>
    public record SystemDocumentListData
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
