using com.etsoo.Utils.Crypto;

namespace Platform.Server.Dto.AuthCode
{
    /// <summary>
    /// Authentication code action
    /// 验证码操作
    /// </summary>
    /// <param name="Id">Id</param>
    /// <param name="Name">Name</param>
    /// <param name="Minutes">Valid minutes</param>
    /// <param name="Kind">Kind</param>
    /// <param name="Length">Length</param>
    /// <param name="Template">Template path</param>
    public record AuthCodeAction(
        short Id,
        string Name,
        short Minutes,
        RandStringKind Kind,
        byte Length,
        string? Template = null
    );
}
