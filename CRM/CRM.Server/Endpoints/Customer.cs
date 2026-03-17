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

            g.MapPut("Create", (ICustomerService service, CustomerCreateRQ rq, CancellationToken cancellationToken) => service.CreateAsync(rq, cancellationToken))
                .WithDescription("Create customer / 创建客户").WithTags("Customer");

            g.MapPost("List", (ICustomerService service, CustomerListRQ rq, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.ListAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Get customer list / 获取客户列表").WithTags("Customer");

            g.MapPost("Query", (ICustomerService service, CustomerQueryRQ rq, CancellationToken cancellationToken) => service.QueryAsync(rq, cancellationToken))
                .WithDescription("Query customer info / 查询客户信息").WithTags("Customer");

            g.MapPost("ReadForSale/{id:long?}", (ICustomerService service, long? id, CancellationToken cancellationToken) => service.ReadForSaleAsync(id, cancellationToken))
                .WithDescription("Get customer info for sale / 获取销售用客户信息").WithTags("Customer");

            g.MapPut("Update", (ICustomerService service, CustomerUpdateRQ rq, CancellationToken cancellationToken) => service.UpdateAsync(rq, cancellationToken))
                .WithDescription("Update customer / 更新客户").WithTags("Customer");

            g.MapGet("UpdateRead/{id:long}", (ICustomerService service, long id, CancellationToken cancellationToken) => service.UpdateReadAsync(id, cancellationToken))
                .WithDescription("Get customer update data / 获取客户更新数据").WithTags("Customer");

            return builder;
        }
    }
}
