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

            g.MapPut("Create", (IProductService service, ProductCreateRQ rq, CancellationToken cancellationToken) => service.CreateAsync(rq, cancellationToken))
                .WithDescription("Create product / 创建产品").WithTags("Product");

            g.MapPost("DuplicateTest", (IProductService service, ProductDuplicateTestRQ rq, CancellationToken cancellationToken) => service.DuplicateTestAsync(rq, cancellationToken))
                .WithDescription("Test for duplicate product / 测试重复的产品").WithTags("Product");

            g.MapPost("List", (IProductService service, ProductListRQ rq, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.ListAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Get product list / 获取产品列表").WithTags("Product");

            g.MapPost("Query", (IProductService service, ProductQueryRQ rq, CancellationToken cancellationToken) => service.QueryAsync(rq, cancellationToken))
                .WithDescription("Query product info / 查询产品信息").WithTags("Product");

            g.MapGet("QueryUnit", (IProductService service,  CancellationToken cancellationToken) => service.QueryUnitAsync(cancellationToken))
                .WithDescription("Query product unit / 查询产品单位").WithTags("Product");

            g.MapPut("Update", (IProductService service, ProductUpdateRQ rq, CancellationToken cancellationToken) => service.UpdateAsync(rq, cancellationToken))
                .WithDescription("Update product / 更新产品").WithTags("Product");

            g.MapGet("UpdateRead/{id:int}", (IProductService service, int id, CancellationToken cancellationToken) => service.UpdateReadAsync(id, cancellationToken))
                .WithDescription("Get product update info / 获取产品更新信息").WithTags("Product");

            g.MapPut("UpdateUnit", (IProductService service, ProductUnitUpdateRQ rq, CancellationToken cancellationToken) => service.UpdateUnitAsync(rq, cancellationToken))
                .WithDescription("Update product unit / 更新产品单位").WithTags("Product");

            return builder;
        }
    }
}
