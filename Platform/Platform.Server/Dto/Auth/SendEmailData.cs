namespace Platform.Server.Dto.Auth
{
    public class SendEmailData
    {
        public required short Action { get; init; }
        public required string Email { get; init; }
        public string? Region { get; init; }
        public string? TimeZone { get; init; }
    }
}
