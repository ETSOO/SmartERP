using com.etsoo.CoreFramework.Models;

namespace Platform.Server.Endpoints.Report.RQ
{
    /// <summary>
    /// Order monthly report query request data
    /// 订单月报表查询请求数据
    /// </summary>
    public record OrderMonthlyReportQueryRQ : QueryLongRQ
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
