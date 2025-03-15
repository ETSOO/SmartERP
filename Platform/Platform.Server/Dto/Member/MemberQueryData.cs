using com.etsoo.CoreFramework.Authentication;
using com.etsoo.CoreFramework.Business;

namespace Platform.Server.Dto.Member
{
    /// <summary>
    /// Member query data
    /// 成员查询数据
    /// </summary>
    public record MemberQueryData
    {
        public required long Id { get; init; }
        public required string Name { get; init; }
        public required UserRole UserRole { get; init; }
        public string? AssignedId { get; init; }
        public bool IsSelf { get; init; }
        public bool IsOwner { get; init; }
        public bool IsEditable { get; init; }
        public int DirectReports { get; init; }
        public EntityStatus Status { get; init; }
        public DateTimeOffset Creation { get; init; }
    }
}
