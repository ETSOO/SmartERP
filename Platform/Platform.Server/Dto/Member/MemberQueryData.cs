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
        public required int Id { get; init; }
        public required string Name { get; init; }
        public required UserRole UserRole { get; init; }
        public string? AssignedId { get; init; }
        public required bool IsSelf { get; init; }
        public required bool IsOwner { get; init; }
        public required bool IsEditable { get; init; }
        public EntityStatus Status { get; init; }
        public required DateTimeOffset Creation { get; init; }
    }
}
