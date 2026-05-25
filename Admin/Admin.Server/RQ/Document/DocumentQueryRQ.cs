using PlatformShared.RQ;

namespace Admin.Server.RQ.Document
{
    /// <summary>
    /// Document query request data
    /// 文档查询请求数据
    /// </summary>
    public record DocumentQueryRQ : SystemDocumentListRQ
    {
        /// <summary>
        /// Organizaton id
        /// 机构编号
        /// </summary>
        public int? OrgId { get; init; }

        /// <summary>
        /// Is system template or not
        /// 是否是系统模板
        /// </summary>
        public bool? SystemTemplate { get; init; }

        /// <summary>
        /// Has parameters or not
        /// 是否有参数
        /// </summary>
        public bool? HasParameters { get; init; }
    }
}
