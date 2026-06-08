namespace PlatformShared.Dto.Document
{
    /// <summary>
    /// Template document kind, the value should not exceed 20 characters
    /// 模板文档类型，值不能超过20个字符
    /// </summary>
    public static class DocumentKind
    {
        /// <summary>
        /// CMS asset check alert
        /// CMS资产检查警告
        /// </summary>
        public const string CmsAssetCheckAlert = "CMSASSETCHECKALERT";

        /// <summary>
        /// CMS asset expiry alert
        /// CMS资产过期警告
        /// </summary>
        public const string CmsAssetExpiryAlert = "CMSASSETEXPIRYALERT";

        /// <summary>
        /// CMS customer data
        /// CMS客户数据
        /// </summary>
        public const string CmsCustomerData = "CMSCUSTOMERDATA";

        /// <summary>
        /// CMS order data
        /// CMS订单数据
        /// </summary>
        public const string CmsOrderData = "CMSORDERDATA";

        /// <summary>
        /// CMS PO data
        /// CMS采购数据
        /// </summary>
        public const string CmsPoData = "CMSPODATA";

        /// <summary>
        /// CMS stock data
        /// CMS库存数据
        /// </summary>
        public const string CmsStockData = "CMSSTOCKDATA";
    }
}
