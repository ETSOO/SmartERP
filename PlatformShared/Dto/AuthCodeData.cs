using System.Text.Json.Serialization;

namespace PlatformShared.Dto
{
    /// <summary>
    /// Auth code data
    /// 验证码数据
    /// </summary>
    [JsonDerivedType(typeof(AuthCodeMemberInvitationData))]
    public record AuthCodeData
    {
    }
}
