using com.etsoo.WebUtils;
using CRM.Server.RQ.Product;
using CRM.Server.Services;

namespace CRM.Server.Endpoints
{
    /// <summary>
    /// Product service APIs
    /// 产品服务API
    /// </summary>
    internal static class Product
    {
        public static RouteGroupBuilder MapProduct(this RouteGroupBuilder builder)
        {
            var g = builder.MapGroup("Product");

            g.MapPost("List", (IProductService service, ProductListRQ rq, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.ListAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Get product list / 获取产品列表").WithTags("Product");

            g.MapPost("Query", (IProductService service, ProductQueryRQ rq, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.QueryAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Query product info / 查询产品信息").WithTags("Product");

            return builder;
        }
    }
}
