namespace Platform.Server.Dto.AuthCode
{
    public record ValidateCodeData
    {
        public required Guid Id { get; init; }
        public required string Code { get; init; }
    }
}
