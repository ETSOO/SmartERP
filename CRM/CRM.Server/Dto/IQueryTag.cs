namespace CRM.Server.Dto
{
    /// <summary>
    /// Query tag interface
    /// 查询标签接口
    /// </summary>
    public interface IQueryTag
    {
        /// <summary>
        /// Tag
        /// 标签
        /// </summary>
        public string? Tag { get; }

        /// <summary>
        /// Tag ID
        /// 标签编号
        /// </summary>
        public int? TagId { get; set; }
    }
}
