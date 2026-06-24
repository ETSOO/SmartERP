using com.etsoo.CoreFramework.Models;

namespace Platform.Server.Endpoints.Report.RQ
{
    /// <summary>
    /// Order monthly report request data
    /// 订单月报表请求数据
    /// </summary>
    public record OrderMonthlyReportRQ
    {
        /// <summary>
        /// Action signed data
        /// 操作签名数据
        /// </summary>
        public required AppActionData Action { get; init; }

        /// <summary>
        /// Year
        /// 年
        /// </summary>
        public int? Year { get; init; }

        /// <summary>
        /// Whether to include last year data
        /// 是否包含去年数据
        /// </summary>
        public bool? HasLastYear { get; init; }
    }
}
