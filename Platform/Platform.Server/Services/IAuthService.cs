using com.etsoo.ApiModel.Auth;
using com.etsoo.CoreFramework.Models;
using com.etsoo.Utils.Actions;
using Platform.Server.Database.Models;

namespace Platform.Server.Services
{
    public interface IAuthService
    {
        IResult GetLogInUrl(IAuthClient client, string? userAgent, string deviceId);
        IResult GetSignUpUrl(IAuthClient client, string? userAgent, string deviceId);
        ValueTask LogInAsync(IAuthClient client, CoreUserIdentifierType type, HttpContext context, CancellationToken cancellationToken = default);
        ValueTask SignUpAsync(IAuthClient client, CoreUserIdentifierType type, HttpContext context, CancellationToken cancellationToken = default);
        ValueTask<IActionResult> WebInitCallAsync(InitCallRQ rq, string identifier);
    }
}