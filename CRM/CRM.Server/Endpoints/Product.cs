using com.etsoo.WebUtils;
using CRM.Server.Dto.Product;
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

            g.MapDelete("Delete/{id:int}", (IProductService service, int id, CancellationToken cancellationToken) => service.DeleteAsync(id, cancellationToken))
                .WithDescription("Delete product / 删除产品").WithTags("Product");

            g.MapPost("DuplicateTest", (IProductService service, ProductDuplicateTestRQ rq, CancellationToken cancellationToken) => service.DuplicateTestAsync(rq, cancellationToken))
                .WithDescription("Test for duplicate product / 测试重复的产品").WithTags("Product");

            g.MapPut("EditBoms", (IProductService service, ProductEditBomsRQ rq, CancellationToken cancellationToken) => service.EditBomsAsync(rq, cancellationToken))
                .WithDescription("Edit product BOMs / 编辑产品物料清单").WithTags("Product");

            g.MapPost("List", (IProductService service, ProductListRQ rq, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.ListAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Get product list / 获取产品列表").WithTags("Product");

            g.MapPost("Query", (IProductService service, ProductQueryRQ rq, CancellationToken cancellationToken) => service.QueryAsync(rq, cancellationToken))
                .WithDescription("Query product info / 查询产品信息").WithTags("Product");

            g.MapPost("QueryForPurchase", (IProductService service, QueryForPurchaseRQ rq, CancellationToken cancellationToken) => service.QueryForPurchaseAsync(rq, true, cancellationToken))
                .WithDescription("Query product for purchase / 查询产品用于采购").WithTags("Product");

            g.MapPost("QueryForSale", (IProductService service, QueryForSaleRQ rq, CancellationToken cancellationToken) => service.QueryForSaleAsync(rq, true, cancellationToken))
                .WithDescription("Query product for sale / 查询产品用于销售").WithTags("Product");

            g.MapGet("QueryUnit", (IProductService service,  CancellationToken cancellationToken) => service.QueryUnitAsync(cancellationToken))
                .WithDescription("Query product unit / 查询产品单位").WithTags("Product");

            g.MapGet("Read/{id:int}", (IProductService service, int id, CancellationToken cancellationToken) => service.ReadAsync(id, cancellationToken))
                .WithDescription("Get product detail / 获取产品详情").WithTags("Product");

            g.MapGet("ReadCustom/{id:int}", (IProductService service, int id, CancellationToken cancellationToken) => service.ReadCustomAsync(id, cancellationToken))
                .WithDescription("Get product custom data / 获取产品自定义数据").WithTags("Product");

            g.MapGet("ReadPrice/{id:int}/{currency}", (IProductService service, int id, string currency, CancellationToken cancellationToken) => service.ReadPriceAsync(id, currency, cancellationToken))
                .WithDescription("Get product price / 获取产品价格").WithTags("Product");

            g.MapPut("Update", (IProductService service, ProductUpdateRQ rq, CancellationToken cancellationToken) => service.UpdateAsync(rq, cancellationToken))
                .WithDescription("Update product / 更新产品").WithTags("Product");

            g.MapPut("UpdateLogo", (IProductService service, ProductUpdateLogoRQ rq, CancellationToken cancellationToken) => service.UpdateLogoAsync(rq, cancellationToken))
                .WithDescription("Update product logo / 更新产品标志").WithTags("Product");

            g.MapGet("UploadLogoAction/{id:int}", (IProductService service, int id, CancellationToken cancellationToken) => service.UploadLogoActionAsync(id, cancellationToken))
                .WithDescription("Get product logo upload action / 获取产品标志上传操作").WithTags("Product");

            g.MapGet("UpdateRead/{id:int}", (IProductService service, int id, CancellationToken cancellationToken) => service.UpdateReadAsync(id, cancellationToken))
                .WithDescription("Get product update info / 获取产品更新信息").WithTags("Product");

            g.MapPut("UpdatePrice/{id:int}", (IProductService service, int id, ProductPriceItem rq, CancellationToken cancellationToken) => service.UpdatePriceAsync(id, rq, cancellationToken))
                .WithDescription("Update product price / 更新产品价格").WithTags("Product");

            g.MapPut("UpdateUnit", (IProductService service, ProductUnitUpdateRQ rq, CancellationToken cancellationToken) => service.UpdateUnitAsync(rq, cancellationToken))
                .WithDescription("Update product unit / 更新产品单位").WithTags("Product");

            return builder;
        }
    }
}
