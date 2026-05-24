using PlatformShared.Dto;

namespace Admin.Server.Dto.Document
{
    /// <summary>
    /// Document view data
    /// 文档浏览数据
    /// </summary>
    public record DocumentViewData : SystemDocumentViewData
    {
        /// <summary>
        /// Organization id
        /// 机构编号
        /// </summary>
        public int? OrgId { get; init; }
    }
}
