namespace Platform.Server.Dto.Auth
{
    public record SendSMSData
    {
        public required short Action { get; init; }
        public required string Mobile { get; init; }
        public string? Region { get; init; }
    }
}
