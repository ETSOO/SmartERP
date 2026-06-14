namespace PlatformShared.Dto.Document.Asset
{
    /// <summary>
    /// Asset check template data
    /// 资产检查模板数据
    /// </summary>
    public record AssetCheckTemplateData
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
        /// Asset check data
        /// 资产检查数据
        /// </summary>
        public required AssetCheckData Asset { get; init; }
    }
}
