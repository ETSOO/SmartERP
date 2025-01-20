using com.etsoo.Utils.Crypto;
using PlatformShared.Database.Models;

namespace Platform.Server.Dto.AuthCode
{
    /// <summary>
    /// Authentication code action item
    /// 验证码操作项目
    /// </summary>
    /// <param name="Id">Id</param>
    /// <param name="Name">Name</param>
    /// <param name="Minutes">Valid minutes</param>
    /// <param name="Kind">Kind</param>
    /// <param name="Length">Length</param>
    /// <param name="Template">Template path</param>
    public record AuthCodeActionItem(
        AuthCodeAction Id,
        string Name,
        short Minutes,
        RandStringKind Kind,
        byte Length,
        string? Template = null
    );
}
