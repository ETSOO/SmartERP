namespace Admin.Server.RQ.Query
{
    public record AuditHistoryRQ : QueryLongRQ
    {
        /// <summary>
        /// User id
        /// 用户编号
        /// </summary>
        public int? UserId { get; init; }

        /// <summary>
        /// Organization id
        /// 机构编号
        /// </summary>
        public int? OrgId { get; init; }

        /// <summary>
        /// Kind
        /// 类型
        /// </summary>
        public string? Kind { get; init; }

        /// <summary>
        /// Target id
        /// 目标编号
        /// </summary>
        public long? TargetId { get; init; }

        /// <summary>
        /// Creation start
        /// 登记开始时间
        /// </summary>
        public DateTime? CreationStart { get; init; }

        /// <summary>
        /// Creation end
        /// 登记结束时间
        /// </summary>
        public DateTime? CreationEnd { get; init; }
    }
}
