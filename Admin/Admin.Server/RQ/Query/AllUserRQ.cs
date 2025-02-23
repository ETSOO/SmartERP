using com.etsoo.CoreFramework.Application;
using com.etsoo.Utils.Actions;

namespace Admin.Server.RQ.Query
{
    /// <summary>
    /// Query all users request data
    /// 查询所有用户请求数据
    /// </summary>
    public record AllUserRQ : QueryIntRQ
    {
        /// <summary>
        /// Organization id
        /// 机构编号
        /// </summary>
        public int? OrgId { get; init; }

        /// <summary>
        /// Inviter id
        /// 邀请人编号
        /// </summary>
        public int? InviterId { get; init; }

        /// <summary>
        /// Is frozen or not
        /// 是否冻结
        /// </summary>
        public bool? IsFrozen { get; init; }

        /// <summary>
        /// Identifier
        /// 识别号
        /// </summary>
        public string? Identifier { get; init; }

        /// <summary>
        /// PIN
        /// 证件号码
        /// </summary>
        public string? Pin { get; init; }

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

            if (Identifier != null && Identifier.Length > 256)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Identifier));
            }

            if (Pin != null && Pin.Length > 20)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Pin));
            }

            return null;
        }
    }
}
