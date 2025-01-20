using System.Text.Json.Serialization;

namespace Platform.Server.Dto.AuthCode
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
