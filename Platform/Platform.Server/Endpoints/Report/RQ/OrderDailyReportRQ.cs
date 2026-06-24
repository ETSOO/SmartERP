using com.etsoo.CoreFramework.Models;

namespace Platform.Server.Endpoints.Report.RQ
{
    /// <summary>
    /// Order daily report request data
    /// 订单日报表请求数据
    /// </summary>
    public record OrderDailyReportRQ
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
        /// Days to cover
        /// 覆盖的天数
        /// </summary>
        public int? Days { get; init; }
    }
}
