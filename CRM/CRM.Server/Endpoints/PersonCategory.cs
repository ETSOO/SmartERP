using com.etsoo.CoreFramework.Models;
using com.etsoo.WebUtils;
using CRM.Server.RQ.PersonCategory;
using CRM.Server.Services;

namespace CRM.Server.Endpoints
{
    /// <summary>
    /// Person category service APIs
    /// 人员分类服务API
    /// </summary>
    internal static class PersonCategory
    {
        public static RouteGroupBuilder MapPersonCategory(this RouteGroupBuilder builder)
        {
            var g = builder.MapGroup("PersonCategory");

            g.MapPut("Create", (IPersonCategoryService service, PersonCategoryCreateRQ rq, CancellationToken cancellationToken) => service.CreateAsync(rq, cancellationToken))
                .WithDescription("Create person category / 创建人员分类").WithTags("PersonCategory");

            g.MapPost("List", (IPersonCategoryService service, PersonCategoryListRQ rq, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.ListAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Get person category list / 获取人员分类列表").WithTags("PersonCategory");

            g.MapPut("Merge", (IPersonCategoryService service, MergeRQ rq, CancellationToken cancellationToken) => service.MergeAsync(rq, cancellationToken))
                .WithDescription("Merge person category / 合并人员分类").WithTags("PersonCategory");

            g.MapPost("Query", (IPersonCategoryService service, PersonCategoryQueryRQ rq, CancellationToken cancellationToken) => service.QueryAsync(rq, cancellationToken))
                .WithDescription("Query person category info / 查询人员分类信息").WithTags("PersonCategory");

            g.MapPut("Sort", (IPersonCategoryService service, Dictionary<int, short> rq, CancellationToken cancellationToken) => service.SortAsync(rq, cancellationToken))
                .WithDescription("Sort person categories / 人员分类排序").WithTags("PersonCategory");

            g.MapPut("Update", (IPersonCategoryService service, PersonCategoryUpdateRQ rq, CancellationToken cancellationToken) => service.UpdateAsync(rq, cancellationToken))
                .WithDescription("Update person category / 更新人员分类").WithTags("PersonCategory");

            g.MapGet("UpdateRead/{id:int}", (IPersonCategoryService service, int id, CancellationToken cancellationToken) => service.UpdateReadAsync(id, cancellationToken))
                .WithDescription("Read person category data for update / 读取用于更新的人员分类数据").WithTags("PersonCategory");

            return builder;
        }
    }
}
