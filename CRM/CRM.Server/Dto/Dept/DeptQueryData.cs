namespace CRM.Server.Dto.Dept
{
    /// <summary>
    /// Department query data
    /// 部门查询数据
    /// </summary>
    public record DeptQueryData
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
