namespace CRM.Server.Dto.PersonCategory
{
    /// <summary>
    /// Person category list data
    /// 人员分类列表数据
    /// </summary>
    public record PersonCategoryListData
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
    }
}
