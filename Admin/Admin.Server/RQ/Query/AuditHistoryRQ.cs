using com.etsoo.CoreFramework.Application;
using com.etsoo.Utils.Actions;
using System.Net;

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
        /// App id
        /// 应用编号
        /// </summary>
        public int? AppId { get; init; }

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
        /// IP
        /// IP地址
        /// </summary>
        public string? Ip { get; init; }

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

        public override IActionResult? Validate()
        {
            var result = base.Validate();
            if (result != null)
            {
                return result;
            }

            if (Kind != null && Kind.Length > 30)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Kind));
            }

            if (Ip != null && !IPAddress.TryParse(Ip, out _))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Ip));
            }

            return null;
        }
    }
}
