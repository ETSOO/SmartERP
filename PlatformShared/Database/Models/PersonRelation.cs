namespace PlatformShared.Database.Models
{
    /// <summary>
    /// Person relation type
    /// 人员关系类型
    /// </summary>
    public enum PersonRelationType : byte
    {
        /// <summary>
        /// Unknown
        /// 未知
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Employee
        /// 雇员
        /// </summary>
        Employee = 1,

        /// <summary>
        /// Supplier
        /// 供应商
        /// </summary>
        Supplier = 3,

        /// <summary>
        /// Media
        /// 媒体
        /// </summary>
        Media = 5,

        /// <summary>
        /// Government
        /// 政府
        /// </summary>
        Government = 7,

        /// <summary>
        /// Shareholder
        /// 股东
        /// </summary>
        Shareholder = 9,

        /// <summary>
        /// Consultant
        /// 顾问
        /// </summary>
        Consultant = 11,

        /// <summary>
        /// Child
        /// 子女
        /// </summary>
        Child = 50,

        /// <summary>
        /// Spouse
        /// 配偶
        /// </summary>
        Spouse = 60,

        /// <summary>
        /// Brother
        /// 兄弟
        /// </summary>
        Brother = 62,

        /// <summary>
        /// Sister
        /// 姐妹
        /// </summary>
        Sister = 64,

        /// <summary>
        /// Parent
        /// 父母
        /// </summary>
        Parent = 80,

        /// <summary>
        /// Grandparent
        /// 祖父母
        /// </summary>
        Grandparent = 90
    }

    /// <summary>
    /// Person relation
    /// 人员关系
    /// </summary>
    public class PersonRelation
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Person id
        /// 人员编号
        /// </summary>
        public long PersonId { get; set; }

        /// <summary>
        /// Contact person id
        /// 联系人编号
        /// </summary>
        public long ContactId { get; set; }

        /// <summary>
        /// Relation type
        /// 关系类型
        /// </summary>
        public PersonRelationType RelationType { get; set; }

        /// <summary>
        /// JSON data
        /// JSON 数据
        /// </summary>
        public string? Data { get; set; }

        /// <summary>
        /// Description
        /// 描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Creation
        /// 登记时间
        /// </summary>
        public DateTimeOffset Creation { get; set; }

        /// <summary>
        /// Contact person
        /// 联系人
        /// </summary>
        public Person Contact { get; set; } = null!;

        /// <summary>
        /// Person
        /// 人员
        /// </summary>
        public Person Person { get; set; } = null!;
    }
}
