using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Authentication;
using Platform.Server.Endpoints.Document.RQ;
using Platform.Server.Services;

namespace Platform.Server.Endpoints.Document
{
    /// <summary>
    /// System document service APIs
    /// 系统文档服务API
    /// </summary>
    public static class Document
    {
        public static RouteGroupBuilder MapDocument(this RouteGroupBuilder builder)
        {
            var g = builder.MapGroup("Document");

            g.MapPost("Create", [Roles(Constants.AdminRoles)] (IDocumentService service, DocumentCreateRQ rq, CancellationToken cancellationToken) => service.CreateAsync(rq, cancellationToken))
                .WithDescription("Create document / 创建文档").WithTags("Document");

            g.MapDelete("Delete/{id:int}", [Roles(Constants.AdminRoles)] (IDocumentService service, int id, CancellationToken cancellationToken) => service.DeleteAsync(id, cancellationToken))
                .WithDescription("Delete document / 删除文档").WithTags("Document");

            g.MapPost("List", (IDocumentService service, DocumentListRQ rq, CancellationToken cancellationToken) => service.ListAsync(rq, cancellationToken))
                .WithDescription("List document data / 文档列表数据").WithTags("Document");

            g.MapPost("Query", (IDocumentService service, DocumentQueryRQ rq, CancellationToken cancellationToken) => service.QueryAsync(rq, cancellationToken))
            .WithDescription("Query documents / 查询文档").WithTags("Document");

            g.MapGet("Read/{id:int}", (IDocumentService service, int id, CancellationToken cancellationToken) => service.ReadAsync(id, cancellationToken))
                .WithDescription("Read document data / 读取文档数据").WithTags("Document");

            g.MapPut("Update", [Roles(Constants.AdminRoles)] (IDocumentService service, DocumentUpdateRQ rq, CancellationToken cancellationToken) => service.UpdateAsync(rq, cancellationToken))
                .WithDescription("Update document / 更新文档").WithTags("Document");

            return builder;
        }
    }
}
