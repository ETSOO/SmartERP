namespace CRM.Server.Dto.Person
{
    /// <summary>
    /// Person tag interface
    /// 人员标签接口
    /// </summary>
    public interface IPersonTag
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
