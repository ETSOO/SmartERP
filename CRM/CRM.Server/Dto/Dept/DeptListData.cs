namespace CRM.Server.Dto.Dept
{
    /// <summary>
    /// Department list data
    /// 部门列表数据
    /// </summary>
    public record DeptListData
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public long Id { get; init; }

        /// <summary>
        /// Name
        /// 名称
        /// </summary>
        public required string Name { get; init; }
    }
}
