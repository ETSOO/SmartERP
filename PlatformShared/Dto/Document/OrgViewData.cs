using PlatformShared.Database.Models;

namespace PlatformShared.Dto.Document
{
    /// <summary>
    /// Organization view data
    /// 机构视图数据
    /// </summary>
    public record OrgViewData
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public long Id { get; init; }

        /// <summary>
        /// Person Id
        /// 实体编号
        /// </summary>
        public long PersonId { get; init; }

        /// <summary>
        /// Uid
        /// 全局唯一标识符
        /// </summary>
        public Guid? Uid { get; init; }

        /// <summary>
        /// Organization name
        /// 组织名称
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// Logo
        /// 图标
        /// </summary>
        public string? Logo { get; init; }

        /// <summary>
        /// Brand
        /// 品牌
        /// </summary>
        public string? Brand { get; init; }

        /// <summary>
        /// Slogan
        /// 标语
        /// </summary>
        public string? Slogan { get; init; }

        /// <summary>
        /// Company seal
        /// 公章
        /// </summary>
        public string? CompanySeal { get; init; }

        /// <summary>
        /// PIN, unique code
        /// PIN，唯一代码
        /// </summary>
        public string? Pin { get; init; }

        /// <summary>
        /// Email
        /// 电子邮件
        /// </summary>
        public string? Email { get; init; }

        /// <summary>
        /// Phone
        /// 电话
        /// </summary>
        public string? Phone { get; init; }

        /// <summary>
        /// Tax ID
        /// 税号
        /// </summary>
        public string? TaxId { get; init; }

        /// <summary>
        /// Website
        /// 网站
        /// </summary>
        public string? Website { get; init; }

        /// <summary>
        /// Address
        /// 地址
        /// </summary>
        public string? Address { get; init; }

        /// <summary>
        /// Region
        /// 所在地区
        /// </summary>
        public required string Region { get; init; }

        /// <summary>
        /// Currencies
        /// 币种
        /// </summary>
        public IEnumerable<string>? Currencies { get; init; }

        /// <summary>
        /// Cultures
        /// 文化
        /// </summary>
        public IEnumerable<string>? Cultures { get; init; }

        /// <summary>
        /// Main customer type
        /// 主要的客户类型
        /// </summary>
        public CustomerType? MainCustomerType { get; init; }

        /// <summary>
        /// Whether has inventory management
        /// 是否有库存管理
        /// </summary>
        public bool HasInventory { get; init; }

        /// <summary>
        /// Default tax rate
        /// 默认税率
        /// </summary>
        public decimal TaxRate { get; init; }

        /// <summary>
        /// Labels
        /// 标签
        /// </summary>
        public List<CustomResourceData> Labels { get; } = [];

        /// <summary>
        /// Get label by key
        /// 通过键获取标签
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Result</returns>
        public string? GetLabel(string key)
        {
            return Labels.FirstOrDefault(l => l.Key == key)?.Title;
        }

        /// <summary>
        /// Get label item by key
        /// 通过键获取标签项
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Result</returns>
        public CustomResourceData? GetLabelItem(string key)
        {
            return Labels.FirstOrDefault(l => l.Key == key);
        }
    }
}
