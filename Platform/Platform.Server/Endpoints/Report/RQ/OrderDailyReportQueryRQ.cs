using com.etsoo.CoreFramework.Models;

namespace Platform.Server.Endpoints.Report.RQ
{
    /// <summary>
    /// Order daily report query request
    /// 订单日报表查询请求
    /// </summary>
    public record OrderDailyReportQueryRQ : QueryLongRQ
    {
        /// <summary>
        /// Action signed data
        /// 操作签名数据
        /// </summary>
        public required AppActionData Action { get; init; }

        /// <summary>
        /// Start date
        /// 开始日期
        /// </summary>
        public DateOnly? StartDate { get; init; }

        /// <summary>
        /// End date
        /// 结束日期
        /// </summary>
        public DateOnly? EndDate { get; init; }
    }
}
