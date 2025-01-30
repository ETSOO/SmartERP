namespace Platform.Server.Endpoints.User.RQ
{
    /// <summary>
    /// Audit history request data
    /// 操作历史请求数据
    /// </summary>
    public record AuditHistoryRQ : QueryLongRQ
    {
        /// <summary>
        /// Device id
        /// 设备编号
        /// </summary>
        public int? DeviceId { get; init; }

        /// <summary>
        /// Kind
        /// 类型
        /// </summary>
        public string? Kind { get; init; }

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
