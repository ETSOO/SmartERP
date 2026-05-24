using Admin.Server.RQ.Document;
using Admin.Server.Services;

namespace Admin.Server.Endpoints
{
    /// <summary>
    /// Document service APIs
    /// 文档服务接口
    /// </summary>
    public static class Document
    {
        public static RouteGroupBuilder MapDocument(this RouteGroupBuilder builder)
        {
            var g = builder.MapGroup("Document");

            g.MapPost("Create", (IDocumentService service, DocumentCreateRQ rq, CancellationToken cancellationToken) => service.CreateAsync(rq, cancellationToken))
                .WithDescription("Create document / 创建文档").WithTags("Document");

            g.MapDelete("Delete/{id:int}", (IDocumentService service, int id, CancellationToken cancellationToken) => service.DeleteAsync(id, cancellationToken))
                .WithDescription("Delete document / 删除文档").WithTags("Document");

            g.MapPost("Query", (IDocumentService service, DocumentQueryRQ rq, CancellationToken cancellationToken) => service.QueryAsync(rq, cancellationToken))
                .WithDescription("Query documents / 查询文档").WithTags("Document");

            g.MapGet("Read/{id:int}", (IDocumentService service, int id, CancellationToken cancellationToken) => service.ReadAsync(id, cancellationToken))
                .WithDescription("Read document / 读取文档").WithTags("Document");

            g.MapPut("Update", (IDocumentService service, DocumentUpdateRQ rq, CancellationToken cancellationToken) => service.UpdateAsync(rq, cancellationToken))
                .WithDescription("Update document / 更新文档").WithTags("Document");

            return builder;
        }
    }
}
