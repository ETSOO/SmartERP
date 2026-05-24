using Platform.Server.Services;
using PlatformShared.RQ;

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

            g.MapPost("List", (IDocumentService service, SystemDocumentListRQ rq, CancellationToken cancellationToken) => service.ListAsync(rq, cancellationToken))
                .WithDescription("List system documents data / 系统文档列表数据").WithTags("Document");

            g.MapGet("Read/{id:int}", (IDocumentService service, int id, CancellationToken cancellationToken) => service.ReadAsync(id, cancellationToken))
                .WithDescription("Read system document data / 读取系统文档数据").WithTags("Document");

            return builder;
        }
    }
}
