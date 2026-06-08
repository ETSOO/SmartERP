namespace PlatformShared.Dto.Document.Order
{
    /// <summary>
    /// Order template data
    /// 订单模板数据
    /// </summary>
    public record OrderTemplateData
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
    }
}
