using com.etsoo.CoreFramework.Business;
using CRM.Server.Dto.Person;

namespace CRM.Server.Dto.Supplier
{
    /// <summary>
    /// Supplier update read data
    /// 更新供应商读取数据
    /// </summary>
    public record SupplierUpdateReadData
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public long Id { get; init; }

        /// <summary>
        /// Is legal person (enterprise)
        /// 是否为法人（企业）
        /// </summary>
        public bool IsLegalPerson { get; init; }

        /// <summary>
        /// Name
        /// 名称 / 姓名
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// Preferred name
        /// 首先名
        /// </summary>
        public string? PreferredName { get; set; }

        /// <summary>
        /// Assigned id
        /// 分配的编号
        /// </summary>
        public string? AssignedId { get; set; }

        /// <summary>
        /// Description
        /// 描述
        /// </summary>
        public string? Description { get; init; }

        /// <summary>
        /// PIN
        /// 身份证号码
        /// </summary>
        public string? Pin { get; init; }

        /// <summary>
        /// Birthday
        /// 生日
        /// </summary>
        public DateTimeOffset? Birthday { get; init; }

        /// <summary>
        /// Categories
        /// 类目
        /// </summary>
        public IEnumerable<int>? Categories { get; init; }

        /// <summary>
        /// Keywords
        /// 关键词
        /// </summary>
        public IEnumerable<string>? Tags { get; init; }

        /// <summary>
        /// Infos
        /// 信息项目
        /// </summary>
        public required IEnumerable<PersonInfoUpdateItem> Infos { get; init; }

        /// <summary>
        /// Status
        /// 状况
        /// </summary>
        public EntityStatus Status { get; init; }
    }
}
