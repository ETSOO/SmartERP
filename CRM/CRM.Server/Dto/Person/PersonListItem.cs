namespace CRM.Server.Dto.Person
{
    /// <summary>
    /// Person list item
    /// 人员列表项
    /// </summary>
    public record PersonListItem
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public long Id { get; init; }

        /// <summary>
        /// Name
        /// 名称 / 姓名
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// Job title
        /// 职务
        /// </summary>
        public string? JobTitle { get; init; }
    }
}
