using com.etsoo.CoreFramework.Models;
using com.etsoo.Utils.Actions;

namespace Platform.Server.Services
{
    public interface IAuthService
    {
        ValueTask<IActionResult> WebInitCallAsync(InitCallRQ rq, string identifier);
    }
}