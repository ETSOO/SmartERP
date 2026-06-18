namespace PlatformShared.Dto.Document.Asset
{
    /// <summary>
    /// Asset template data
    /// 资产模板数据
    /// </summary>
    public record AssetTemplateData : IDocumentTemplateData
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
        public required OrgViewData Org { get; init; }

        /// <summary>
        /// Asset view data
        /// 资产视图数据
        /// </summary>
        public required AssetViewData Asset { get; init; }

        /// <summary>
        /// Target name
        /// 目标对象名称
        /// </summary>
        public string TargetName => Asset.Sn;
    }
}
