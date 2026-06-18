namespace PlatformShared.Dto.Document
{
    /// <summary>
    /// Document template data interface
    /// 文档模板数据接口
    /// </summary>
    public interface IDocumentTemplateData
    {
        /// <summary>
        /// Subject
        /// 主题
        /// </summary>
        public string? Subject { get; set; }

        /// <summary>
        /// Organization view data
        /// 机构视图数据
        /// </summary>
        public OrgViewData Org { get; }

        /// <summary>
        /// Target name
        /// 目标对象名称
        /// </summary>
        public string TargetName { get; }
    }
}
