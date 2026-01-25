using PlatformShared.Database.Models;

namespace CRM.Server.RQ.PersonInfo
{
    /// <summary>
    /// Person info query request data
    /// 查询人员信息请求数据
    /// </summary>
    public record PersonInfoQueryRQ : QueryIntRQ
    {
        /// <summary>
        /// Person id
        /// 人员编号
        /// </summary>
        public long PersonId { get; init; }

        /// <summary>
        /// Identifier
        /// 标识
        /// </summary>
        public string? Identifier { get; init; }

        /// <summary>
        /// Kind
        /// 类型
        /// </summary>
        public PersonInfoKind? Kind { get; init; }

        /// <summary>
        /// Is subscribed
        /// 是否订阅
        /// </summary>
        public bool? Subscribed { get; init; }

        /// <summary>
        /// Is default
        /// 是否默认
        /// </summary>
        public bool? IsDefault { get; init; }

        /// <summary>
        /// Is verified
        /// 是否验证
        /// </summary>
        public bool? IsVerified { get; init; }
    }
}
