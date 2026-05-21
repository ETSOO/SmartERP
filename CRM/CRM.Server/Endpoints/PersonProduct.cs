using CRM.Server.RQ.PersonProduct;
using CRM.Server.Services;

namespace CRM.Server.Endpoints
{
    /// <summary>
    /// Person product service APIs
    /// 人员个性化产品服务API
    /// </summary>
    internal static class PersonProduct
    {
        public static RouteGroupBuilder MapPersonProduct(this RouteGroupBuilder builder)
        {
            var g = builder.MapGroup("PersonProduct");

            g.MapPost("Create", (IPersonProductService service, PersonProductCreateRQ rq, CancellationToken cancellationToken) => service.CreateAsync(rq, cancellationToken))
                .WithDescription("Create person product / 创建人员个性化产品").WithTags("PersonProduct");

            g.MapDelete("Delete/{productId:int}/{personId:long}", (IPersonProductService service, int productId, long personId, CancellationToken cancellationToken) => service.DeleteAsync(personId, productId, cancellationToken))
                .WithDescription("Delete person product / 删除人员个性化产品").WithTags("PersonProduct");

            g.MapPost("Query", (IPersonProductService service, PersonProductQueryRQ rq, CancellationToken cancellationToken) => service.QueryAsync(rq, cancellationToken))
                .WithDescription("Query person products / 查询人员个性化产品").WithTags("PersonProduct");

            g.MapPut("Update", (IPersonProductService service, PersonProductUpdateRQ rq, CancellationToken cancellationToken) => service.UpdateAsync(rq, cancellationToken))
                .WithDescription("Update person product / 更新人员个性化产品").WithTags("PersonProduct");

            g.MapGet("UpdateRead/{productId:int}/{personId:long}", (IPersonProductService service, int productId, long personId, CancellationToken cancellationToken) => service.UpdateReadAsync(personId, productId, cancellationToken))
                .WithDescription("Read person product data for update / 读取用于更新的人员个性化产品数据").WithTags("PersonProduct");

            return builder;
        }
    }
}
