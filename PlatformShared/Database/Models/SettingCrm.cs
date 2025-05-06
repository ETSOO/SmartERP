namespace PlatformShared.Database.Models
{
    /// <summary>
    /// Customer type
    /// 客户类型
    /// </summary>
    public enum CustomerType : byte
    {
        /// <summary>
        /// Business
        /// 企业
        /// </summary>
        Business = 1,

        /// <summary>
        /// Government
        /// 政府
        /// </summary>
        Government = 3,

        /// <summary>
        /// School
        /// 学校
        /// </summary>
        School = 5,

        /// <summary>
        /// Individual
        /// 个人
        /// </summary>
        Individual = 10
    }

    /// <summary>
    /// CRM Settings
    /// 客户关系管理设置
    /// </summary>
    public class SettingCrm
    {
        /// <summary>
        /// Organization Id
        /// 机构编号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Organization person id
        /// 机构人员编号
        /// </summary>
        public long PersonId { get; set; }

        /// <summary>
        /// Main customer type
        /// 主要客户类型
        /// </summary>
        public CustomerType MainCustomerType { get; set; }

        /// <summary>
        /// Currencies
        /// 币种
        /// </summary>
        public List<string> Currencies { get; set; } = default!;

        /// <summary>
        /// Supplier currencies
        /// 供应商币种
        /// </summary>
        public List<string> SupplierCurrencies { get; set; } = default!;

        /// <summary>
        /// Cultures
        /// 文化
        /// </summary>
        public List<string> Cultures { get; set; } = default!;

        /// <summary>
        /// Whether has inventory management
        /// 是否有库存管理
        /// </summary>
        public bool HasInventory { get; set; }

        /// <summary>
        /// Organization
        /// 机构
        /// </summary>
        public CoreOrganization Organization { get; set; } = null!;

        /// <summary>
        /// Person of the organization
        /// 机构的人员
        /// </summary>
        public Person Person { get; set; } = null!;
    }
}
