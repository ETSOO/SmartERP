using com.etsoo.WebUtils;
using CRM.Server.RQ.Promotion;
using CRM.Server.Services;

namespace CRM.Server.Endpoints
{
    /// <summary>
    /// Promotion service APIs
    /// 促销服务API
    /// </summary>
    internal static class Promotion
    {
        public static RouteGroupBuilder MapPromotion(this RouteGroupBuilder builder)
        {
            var g = builder.MapGroup("Promotion");

            g.MapPost("Create", (IPromotionService service, PromotionCreateRQ rq, CancellationToken cancellationToken) => service.CreateAsync(rq, cancellationToken))
                .WithDescription("Create promotion / 创建促销").WithTags("Promotion");

            g.MapPost("List", (IPromotionService service, PromotionListRQ rq, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.ListAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Get promotion list / 获取促销列表").WithTags("Promotion");

            g.MapPost("Query", (IPromotionService service, PromotionQueryRQ rq, CancellationToken cancellationToken) => service.QueryAsync(rq, cancellationToken))
                .WithDescription("Query promotion info / 查询促销信息").WithTags("Promotion");

            g.MapPut("Sort", (IPromotionService service, Dictionary<int, short> rq, CancellationToken cancellationToken) => service.SortAsync(rq, cancellationToken))
                .WithDescription("Sort promotions / 排序促销").WithTags("Promotion");

            g.MapPut("Update", (IPromotionService service, PromotionUpdateRQ rq, CancellationToken cancellationToken) => service.UpdateAsync(rq, cancellationToken))
                .WithDescription("Update promotion / 更新促销").WithTags("Promotion");

            g.MapGet("UpdateRead/{id:int}", (IPromotionService service, int id, CancellationToken cancellationToken) => service.UpdateReadAsync(id, cancellationToken))
                .WithDescription("Read promotion data for update / 读取用于更新的促销数据").WithTags("Promotion");

            return builder;
        }
    }
}
