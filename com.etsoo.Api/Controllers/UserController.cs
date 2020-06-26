using com.etsoo.Api.Helpers;
using com.etsoo.SmartERP.Applications;
using com.etsoo.SmartERP.User;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace com.etsoo.Api.Controllers
{
    /// <summary>
    /// User controller
    /// 用户控制器
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class UserController : LoginController<UserSerivce, int>
    {
        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="distributedCache">Distributed cache</param>
        /// <param name="httpContextAccessor">Http context accessor</param>
        public UserController(MainApp app, IDistributedCache distributedCache, IHttpContextAccessor httpContextAccessor)
            : base(UserSerivce.Create(app, IdentifyUser.Create(httpContextAccessor.HttpContext.User, httpContextAccessor.HttpContext.Connection.RemoteIpAddress)), distributedCache)
        {
        }
    }
}