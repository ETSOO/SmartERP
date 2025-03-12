namespace Platform.Server.Endpoints.Member.RQ
{
    /// <summary>
    /// Member adjust report to request data
    /// 成员调整汇报对象请求数据
    /// </summary>
    public record MemberAdjustReportToRQ
    {
        /// <summary>
        /// Old id
        /// 旧编号
        /// </summary>
        public long OldId { get; init; }

        /// <summary>
        /// New id
        /// 新编号
        /// </summary>
        public long NewId { get; init; }
    }
}
