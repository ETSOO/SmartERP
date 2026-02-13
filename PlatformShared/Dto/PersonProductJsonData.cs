namespace PlatformShared.Dto
{
    /// <summary>
    /// Person product JSON data. For EF modeling, properties should be get/set and array with List
    /// 人员个性化产品 JSON 数据
    /// </summary>
    public record PersonProductJsonData
    {
        /// <summary>
        /// Cultures
        /// 文化
        /// </summary>
        public List<ProductCustomData>? Cultures { get; set; }

        /// <summary>
        /// Prices
        /// 价格
        /// </summary>
        public List<ProductSimplePriceItem>? Prices { get; set; }
    }
}
