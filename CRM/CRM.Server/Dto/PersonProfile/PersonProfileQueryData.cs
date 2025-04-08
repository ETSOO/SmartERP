using com.etsoo.CoreFramework.Business;
using PlatformShared.Database.Models;

namespace CRM.Server.Dto.PersonProfile
{
    /// <summary>
    /// Person profile query data
    /// 人员档案查询数据
    /// </summary>
    public record PersonProfileQueryData
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public long Id { get; init; }

        /// <summary>
        /// Kind
        /// 类型
        /// </summary>
        public PersonProfileKind? Kind { get; init; }

        /// <summary>
        /// Title
        /// 标题
        /// </summary>
        public required string Title { get; init; }

        /// <summary>
        /// User name
        /// 用户姓名
        /// </summary>
        public required string UserName { get; init; }

        /// <summary>
        /// Importance
        /// 重要性
        /// </summary>
        public PersonProfileImportance? Importance { get; init; }

        /// <summary>
        /// Happen date
        /// 发生日期
        /// </summary>
        public DateTimeOffset? HappenDate { get; init; }

        /// <summary>
        /// Happen date end
        /// 发生日期结束
        /// </summary>
        public DateTimeOffset? HappenDateEnd { get; init; }

        /// <summary>
        /// Is the author of self
        /// 自己是否为作者
        /// </summary>
        public bool IsSelf { get; init; }

        /// <summary>
        /// Status
        /// 状况
        /// </summary>
        public EntityStatus Status { get; init; }

        /// <summary>
        /// Creation
        /// 登记时间
        /// </summary>
        public DateTimeOffset Creation { get; init; }
    }
}
