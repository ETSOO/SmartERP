namespace PlatformShared.Dto.Document.Asset
{
    /// <summary>
    /// Asset check template data
    /// 资产检查模板数据
    /// </summary>
    public record AssetCheckTemplateData : IDocumentTemplateData
    {
        /// <summary>
        /// Subject
        /// 主题
        /// </summary>
        public string? Subject { get; set; }

        /// <summary>
        /// Web URL
        /// Web 网址
        /// </summary>
        public string WebUrl { get; set; } = string.Empty;

        /// <summary>
        /// Organization view data
        /// 机构视图数据
        /// </summary>
        public required OrgViewData Org { get; init; }

        /// <summary>
        /// Asset check data
        /// 资产检查数据
        /// </summary>
        public required AssetCheckData Asset { get; init; }

        /// <summary>
        /// Target name
        /// 目标对象名称
        /// </summary>
        public string TargetName => Asset.Sn;
    }
}
