using com.etsoo.CoreFramework.Application;
using com.etsoo.Utils.Actions;

namespace Admin.Server.RQ.Query
{
    /// <summary>
    /// Query all organizations request data
    /// 查询所有机构请求数据
    /// </summary>
    public record AllOrgRQ : QueryIntRQ
    {
        /// <summary>
        /// Parent org. ID
        /// 父机构编号
        /// </summary>
        public int? ParentId { get; init; }

        /// <summary>
        /// Owner id
        /// 所有人编号
        /// </summary>
        public int? OwnerId { get; init; }

        /// <summary>
        /// PIN
        /// 唯一编号
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

            if (Pin != null && Pin.Length > 20)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Pin));
            }

            return null;
        }
    }
}
