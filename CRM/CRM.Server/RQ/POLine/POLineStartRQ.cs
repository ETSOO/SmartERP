namespace CRM.Server.RQ.POLine
{
    /// <summary>
    /// Start to execute request data
    /// 开始执行请求数据
    /// </summary>
    public record POLineStartRQ
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public long Id { get; init; }

        /// <summary>
        /// User id
        /// 用户编号
        /// </summary>
        public long? UserId { get; init; }

        /// <summary>
        /// Init start ime
        /// 初始化开始时间
        /// </summary>
        public bool? InitStart { get; init; }
    }
}
