namespace CRM.Server.RQ.Person
{
    /// <summary>
    /// Choose persons request data
    /// 选择人员请求数据
    /// </summary>
    public record ChoosePersonsRQ
    {
        /// <summary>
        /// Person Id
        /// 人员编号
        /// </summary>
        public long PersonId { get; init; }

        /// <summary>
        /// Maximum items
        /// 最大项数
        /// </summary>
        public int MaxItems { get; init; }
    }
}
