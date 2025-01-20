namespace Platform.Server.Dto.Member
{
    public record MemberInvitationData
    {
        public required string Email { get; init; }
        public required string Inviter { get; init; }
        public required string OrgName { get; init; }
        public required bool IsExpired { get; init; }
        public required bool UserExists { get; init; }
    }
}
