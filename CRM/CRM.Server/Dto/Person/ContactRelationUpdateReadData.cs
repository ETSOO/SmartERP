using PlatformShared.Database.Models;

namespace CRM.Server.Dto.Person
{
    /// <summary>
    /// Contact relation update read data
    /// 联系人关系更新读取数据
    /// </summary>
    public record ContactRelationUpdateReadData
    {
        /// <summary>
        /// Relation type
        /// 关系类型
        /// </summary>
        public PersonRelationType RelationType { get; init; }

        /// <summary>
        /// Description
        /// 描述
        /// </summary>
        public string? Description { get; init; }

        /// <summary>
        /// JSON data
        /// JSON 数据
        /// </summary>
        public string? Data { get; init; }

        /// <summary>
        /// Creation
        /// 登记时间
        /// </summary>
        public DateTimeOffset Creation { get; init; }
    }
}
