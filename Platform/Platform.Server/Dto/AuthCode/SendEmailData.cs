using PlatformShared.Database.Models;

namespace Platform.Server.Dto.AuthCode
{
    public class SendEmailData
    {
        public required AuthCodeAction Action { get; init; }
        public required string Email { get; init; }
        public string? Region { get; init; }
        public string? TimeZone { get; init; }
    }

    public class SendEmailData<D> : SendEmailData where D : AuthCodeData
    {
        public required D Data { get; init; }
    }
}
