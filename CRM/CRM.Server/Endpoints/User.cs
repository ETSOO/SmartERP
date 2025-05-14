using com.etsoo.WebUtils;
using CRM.Server.RQ.User;
using CRM.Server.Services;

namespace CRM.Server.Endpoints
{
    /// <summary>
    /// User service APIs
    /// 用户服务API
    /// </summary>
    internal static class User
    {
        public static RouteGroupBuilder MapUser(this RouteGroupBuilder builder)
        {
            var g = builder.MapGroup("User");

            g.MapPost("List", (IUserService service, UserListRQ rq, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.ListAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Get user list / 获取用户列表").WithTags("User");

            g.MapPost("Query", (IUserService service, UserQueryRQ rq, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.QueryAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Query user info / 查询用户信息").WithTags("User");

            return builder;
        }
    }
}
