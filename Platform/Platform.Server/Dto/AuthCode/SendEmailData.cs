using PlatformShared.Database.Models;
using PlatformShared.Dto;

namespace Platform.Server.Dto.AuthCode
{
    public class SendEmailData
    {
        public required AuthCodeAction Action { get; init; }
        public required string Email { get; init; }
        public required string Region { get; init; }
        public required string TimeZone { get; init; }
    }

    public class SendEmailData<D> : SendEmailData where D : AuthCodeData
    {
        public required D Data { get; init; }
    }
}
