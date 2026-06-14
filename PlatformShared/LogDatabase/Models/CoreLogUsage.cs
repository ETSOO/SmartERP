namespace PlatformShared.LogDatabase.Models
{
    /// <summary>
    /// Core log usage
    /// 核心日志使用量
    /// </summary>
    public class CoreLogUsage
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Organization id
        /// 机构编号
        /// </summary>
        public int OrganizationId { get; set; }

        /// <summary>
        /// Period
        /// 区间
        /// </summary>
        public int Period { get; set; }

        /// <summary>
        /// Qty.
        /// 数量
        /// </summary>
        public int Qty { get; set; }
    }
}
