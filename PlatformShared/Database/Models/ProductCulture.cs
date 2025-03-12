namespace PlatformShared.Database.Models
{
    /// <summary>
    /// Product culture
    /// 产品文化
    /// </summary>
    public class ProductCulture
    {
        /// <summary>
        /// Product id
        /// 产品编号
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Culture
        /// 文化
        /// </summary>
        public string Culture { get; set; } = default!;

        /// <summary>
        /// Custom name
        /// 自定义名称
        /// </summary>
        public string Name { get; set; } = default!;

        /// <summary>
        /// Custom description
        /// 自定义描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Custom JSON data
        /// 自定义 JSON 数据
        /// </summary>
        public string? Data { get; set; }
    }
}
