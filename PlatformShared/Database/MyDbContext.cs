using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using PlatformShared.Database.Models;
using PlatformShared.Database.Models.Configurations;

namespace PlatformShared.Database
{
    /// <summary>
    /// My database context
    /// https://learn.microsoft.com/en-us/ef/core/modeling/
    /// 1. Using the OnModelCreating method (fluent API)
    /// 2. The other way is Data annotation
    /// 我的数据库上下文
    /// </summary>
    public partial class MyDbContext : DbContext
    {
        /// <summary>
        /// Addresses
        /// 地址
        /// </summary>
        public required DbSet<Address> Addresses { get; set; }

        /// <summary>
        /// Core applications
        /// 核心应用
        /// </summary>
        public required DbSet<CoreApp> CoreApps { get; set; }

        /// <summary>
        /// Authorization codes
        /// 授权码
        /// </summary>
        public required DbSet<CoreAuthCode> CoreAuthCodes { get; set; }

        /// <summary>
        /// Core organizations
        /// 核心机构
        /// </summary>
        public required DbSet<CoreOrganization> CoreOrganizations { get; set; }

        /// <summary>
        /// Core organization applications
        /// 核心机构应用
        /// </summary>
        public required DbSet<CoreOrganizationApp> CoreOrganizationApps { get; set; }

        /// <summary>
        /// Core users
        /// 核心用户
        /// </summary>
        public required DbSet<CoreUser> CoreUsers { get; set; }

        /// <summary>
        /// Core user devices
        /// 核心用户设备
        /// </summary>
        public required DbSet<CoreUserDevice> CoreUserDevices { get; set; }

        /// <summary>
        /// Core user device tokens
        /// 核心用户设备令牌
        /// </summary>
        public required DbSet<CoreUserDeviceToken> CoreUserDeviceTokens { get; set; }

        /// <summary>
        /// Core user identifiers for login
        /// 核心用户登录编号
        /// </summary>
        public required DbSet<CoreUserIdentifier> CoreUserIdentifiers { get; set; }

        /// <summary>
        /// Feature (custom) cultures
        /// 特征（自定义）文化
        /// </summary>
        public required DbSet<FeatureCulture> FeatureCultures { get; set; }

        /// <summary>
        /// Feature keywords
        /// 特征关键词
        /// </summary>
        public required DbSet<FeatureKeyword> FeatureKeywords { get; set; }

        /// <summary>
        /// Order, PO or transaction
        /// 订单，采购或交易
        /// </summary>
        public required DbSet<OrderHeader> OrderHeaders { get; set; }

        /// <summary>
        /// Order lines
        /// 订单行
        /// </summary>
        public required DbSet<OrderLine> OrderLines { get; set; }

        /// <summary>
        /// Permission groups
        /// 权限组
        /// </summary>
        public required DbSet<PermissionGroup> PermissionGroups { get; set; }

        /// <summary>
        /// Permission items
        /// 权限项
        /// </summary>
        public required DbSet<PermissionItem> PermissionItems { get; set; }

        /// <summary>
        /// Individuals or companies or contacts
        /// 个人或企业或联系人
        /// </summary>
        public required DbSet<Person> Persons { get; set; }

        /// <summary>
        /// Person assets
        /// 个人资产
        /// </summary>
        public required DbSet<PersonAsset> PersonAssets { get; set; }

        /// <summary>
        /// Person categories
        /// 个人类目
        /// </summary>
        public required DbSet<PersonCategory> PersonCategories { get; set; }

        /// <summary>
        /// Person information
        /// 个人信息
        /// </summary>
        public required DbSet<PersonInfo> PersonInfos { get; set; }

        /// <summary>
        /// Person products
        /// 个人产品
        /// </summary>
        public required DbSet<PersonProduct> PersonProducts { get; set; }

        /// <summary>
        /// Person profiles
        /// 个人档案
        /// </summary>
        public required DbSet<PersonProfile> PersonProfiles { get; set; }

        /// <summary>
        /// Person profile attachments
        /// 个人档案附件
        /// </summary>
        public required DbSet<PersonProfileAttachment> PersonProfileAttachments { get; set; }

        /// <summary>
        /// Person profile links
        /// 个人档案链接
        /// </summary>
        public required DbSet<PersonProfileLink> PersonProfileLinks { get; set; }

        /// <summary>
        /// Person relations
        /// 人员关系
        /// </summary>
        public required DbSet<PersonRelation> PersonRelations { get; set; }

        /// <summary>
        /// Products
        /// 产品
        /// </summary>
        public required DbSet<Product> Products { get; set; }

        /// <summary>
        /// Product categories
        /// 产品类目
        /// </summary>
        public required DbSet<ProductCategory> ProductCategories { get; set; }

        /// <summary>
        /// Product cultures
        /// 产品文化
        /// </summary>
        public required DbSet<ProductCulture> ProductCultures { get; set; }

        /// <summary>
        /// Product prices
        /// 产品价格
        /// </summary>
        public required DbSet<ProductPrice> ProductPrices { get; set; }

        /// <summary>
        /// Product units
        /// 产品单位
        /// </summary>
        public required DbSet<ProductUnit> ProductUnits { get; set; }

        /// <summary>
        /// Promotions
        /// 促销
        /// </summary>
        public required DbSet<Promotion> Promotions { get; set; }

        /// <summary>
        /// CRM settings
        /// 客户关系管理设置
        /// </summary>
        public required DbSet<SettingCrm> SettingCrms { get; set; }

        /// <summary>
        /// Is sensitive data logging enabled
        /// 敏感数据日志是否启用
        /// </summary>
        public readonly bool IsSensitiveDataLoggingEnabled;

        public MyDbContext(DbContextOptions<MyDbContext> options)
            : base(options)
        {
            IsSensitiveDataLoggingEnabled =  options.GetExtension<CoreOptionsExtension>().IsSensitiveDataLoggingEnabled;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Register custom functions
            MyDbFunctions.Register(modelBuilder);

            modelBuilder.ApplyConfiguration(new AddressConfiguration());
            modelBuilder.ApplyConfiguration(new CoreAppConfiguration());
            modelBuilder.ApplyConfiguration(new CoreAuthCodeConfiguration());
            modelBuilder.ApplyConfiguration(new CoreOrganizationConfiguration());
            modelBuilder.ApplyConfiguration(new CoreOrganizationAppConfiguration());
            modelBuilder.ApplyConfiguration(new CoreUserConfiguration());
            modelBuilder.ApplyConfiguration(new CoreUserDeviceConfiguration());
            modelBuilder.ApplyConfiguration(new CoreUserDeviceTokenConfiguration());
            modelBuilder.ApplyConfiguration(new CoreUserIdentifierConfiguration());
            modelBuilder.ApplyConfiguration(new FeatureCultureConfiguration());
            modelBuilder.ApplyConfiguration(new FeatureKeywordConfiguration());
            modelBuilder.ApplyConfiguration(new OrderHeaderConfiguration());
            modelBuilder.ApplyConfiguration(new OrderLineConfiguration());
            modelBuilder.ApplyConfiguration(new PermissionGroupConfiguration());
            modelBuilder.ApplyConfiguration(new PermissionItemConfiguration());
            modelBuilder.ApplyConfiguration(new PersonConfiguration());
            modelBuilder.ApplyConfiguration(new PersonAssetConfiguration());
            modelBuilder.ApplyConfiguration(new PersonCategoryConfiguration());
            modelBuilder.ApplyConfiguration(new PersonInfoConfiguration());
            modelBuilder.ApplyConfiguration(new PersonProductConfiguration());
            modelBuilder.ApplyConfiguration(new PersonProfileConfiguration());
            modelBuilder.ApplyConfiguration(new PersonProfileAttachmentConfiguration());
            modelBuilder.ApplyConfiguration(new PersonProfileLinkConfiguration());
            modelBuilder.ApplyConfiguration(new PersonRelationConfiguration());
            modelBuilder.ApplyConfiguration(new ProductConfiguration());
            modelBuilder.ApplyConfiguration(new ProductCategoryConfiguration());
            modelBuilder.ApplyConfiguration(new ProductCultureConfiguration());
            modelBuilder.ApplyConfiguration(new ProductPriceConfiguration());
            modelBuilder.ApplyConfiguration(new ProductUnitConfiguration());
            modelBuilder.ApplyConfiguration(new PromotionConfiguration());
            modelBuilder.ApplyConfiguration(new SettingCrmConfiguration());
        }
    }
}
