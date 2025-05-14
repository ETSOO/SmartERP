using com.etsoo.WebUtils;
using CRM.Server.RQ.Customer;
using CRM.Server.Services;

namespace CRM.Server.Endpoints
{
    /// <summary>
    /// Customer service APIs
    /// 客户服务API
    /// </summary>
    internal static class Customer
    {
        public static RouteGroupBuilder MapCustomer(this RouteGroupBuilder builder)
        {
            var g = builder.MapGroup("Customer");

            g.MapPost("List", (ICustomerService service, CustomerListRQ rq, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.ListAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Get customer list / 获取客户列表").WithTags("Customer");

            g.MapPost("Query", (ICustomerService service, CustomerQueryRQ rq, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.QueryAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Query customer info / 查询客户信息").WithTags("Customer");

            return builder;
        }
    }
}
