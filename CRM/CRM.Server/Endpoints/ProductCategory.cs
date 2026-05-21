using com.etsoo.CoreFramework.Models;
using com.etsoo.WebUtils;
using CRM.Server.RQ.ProductCategory;
using CRM.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Server.Endpoints
{
    /// <summary>
    /// Product category service APIs
    /// 产品分类服务API
    /// </summary>
    internal static class ProductCategory
    {
        public static RouteGroupBuilder MapProductCategory(this RouteGroupBuilder builder)
        {
            var g = builder.MapGroup("ProductCategory");

            g.MapPost("Create", (IProductCategoryService service, ProductCategoryCreateRQ rq, CancellationToken cancellationToken) => service.CreateAsync(rq, cancellationToken))
                .WithDescription("Create product category / 创建产品分类").WithTags("ProductCategory");

            g.MapPost("DuplicateTest", (IProductCategoryService service, ProductCategoryDuplicateTestRQ rq, CancellationToken cancellationToken) => service.DuplicateTestAsync(rq, cancellationToken))
                .WithDescription("Test for duplicate product category / 测试重复的产品分类").WithTags("ProductCategory");

            g.MapPost("GetAttributes", (IProductCategoryService service, int[] ids, CancellationToken cancellationToken) => service.GetAttributesAsync(ids, cancellationToken))
                .WithDescription("Get product category attributes / 获取产品分类属性").WithTags("ProductCategory");

            g.MapPost("List", (IProductCategoryService service, ProductCategoryListRQ rq, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.ListAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Get product category list / 获取产品分类列表").WithTags("ProductCategory");

            g.MapPut("Merge", (IProductCategoryService service, MergeRQ rq, CancellationToken cancellationToken) => service.MergeAsync(rq, cancellationToken))
                .WithDescription("Merge product category / 合并产品分类").WithTags("ProductCategory");

            g.MapPost("Query", (IProductCategoryService service, ProductCategoryQueryRQ rq, CancellationToken cancellationToken) => service.QueryAsync(rq, cancellationToken))
                .WithDescription("Query product category info / 查询产品分类信息").WithTags("ProductCategory");

            g.MapPut("Sort", (IProductCategoryService service, Dictionary<int, short> rq, CancellationToken cancellationToken) => service.SortAsync(rq, cancellationToken))
                .WithDescription("Sort product categories / 产品分类排序").WithTags("ProductCategory");

            g.MapPut("Update", (IProductCategoryService service, ProductCategoryUpdateRQ rq, CancellationToken cancellationToken) => service.UpdateAsync(rq, cancellationToken))
                .WithDescription("Update product category / 更新产品分类").WithTags("ProductCategory");

            g.MapGet("UpdateRead/{id:int}", (IProductCategoryService service, int id, CancellationToken cancellationToken) => service.UpdateReadAsync(id, cancellationToken))
                .WithDescription("Read product category data for update / 读取用于更新的产品分类数据").WithTags("ProductCategory");

            return builder;
        }
    }
}
