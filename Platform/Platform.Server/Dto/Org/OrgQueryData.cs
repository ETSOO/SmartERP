using com.etsoo.CoreFramework.Business;

namespace Platform.Server.Dto.Org
{
    /// <summary>
    /// Organization query data
    /// 机构查询数据
    /// </summary>
    public record OrgQueryData
    {
        public required int Id { get; init; }
        public required string Name { get; init; }
        public required bool IsOwner { get; init; }
        public string? Brand { get; init; }
        public string? Pin { get; init; }
        public int? ParentId { get; init; }
        public EntityStatus Status { get; init; }
        public DateTimeOffset Creation { get; init; }
        public EntityStatus UserStatus { get; init; }
        public bool IsUserExpired { get; init; }
    }
}
