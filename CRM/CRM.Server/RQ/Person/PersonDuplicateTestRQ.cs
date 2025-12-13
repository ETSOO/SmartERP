using PlatformShared.Database.Models;

namespace CRM.Server.RQ.Person
{
    /// <summary>
    /// Person duplicate test request data
    /// 人员重复测试请求数据
    /// </summary>
    public record PersonDuplicateTestRQ
    {
        /// <summary>
        /// Excluded id
        /// 排除的编号
        /// </summary>
        public long? ExcludedId { get; init; }

        /// <summary>
        /// Name
        /// 名称 / 姓名
        /// </summary>
        public string? Name { get; init; }

        /// <summary>
        /// Info kind
        /// 信息类型
        /// </summary>
        public PersonInfoKind? InfoKind { get; init; }

        /// <summary>
        /// Info identifier
        /// 信息标识
        /// </summary>
        public string? Identifier { get; init; }

        /// <summary>
        /// Address
        /// 地址
        /// </summary>
        public string? Address { get; init; }
    }
}
