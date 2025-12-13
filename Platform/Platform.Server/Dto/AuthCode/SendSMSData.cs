using PlatformShared.Database.Models;
using PlatformShared.Dto;

namespace Platform.Server.Dto.AuthCode
{
    public record SendSMSData
    {
        public required AuthCodeAction Action { get; init; }
        public required string Mobile { get; init; }
        public required string Region { get; init; }
    }

    public record SendSMSData<D> : SendSMSData where D : AuthCodeData
    {
        public required D Data { get; init; }
    }
}
