namespace CRM.Server.Dto.PersonProfile
{
    /// <summary>
    /// Person profile list data
    /// 人员档案列表数据
    /// </summary>
    public record PersonProfileListData
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public long Id { get; init; }

        /// <summary>
        /// Title
        /// 标题
        /// </summary>
        public required string Title { get; init; }

        /// <summary>
        /// Creation
        /// 登记时间
        /// </summary>
        public DateTimeOffset Creation { get; init; }
    }
}
