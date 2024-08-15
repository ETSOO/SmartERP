namespace Platform.Server.Dto.Auth
{
    public record ValidateCodeData
    {
        public required Guid Id { get; init; }
        public required string Code { get; init; }
    }
}
