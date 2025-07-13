using com.etsoo.WebUtils;
using CRM.Server.RQ.PersonProfile;
using CRM.Server.Services;

namespace CRM.Server.Endpoints
{
    /// <summary>
    /// Person profile service APIs
    /// 人员档案服务API
    /// </summary>
    internal static class PersonProfile
    {
        public static RouteGroupBuilder MapPersonProfile(this RouteGroupBuilder builder)
        {
            var g = builder.MapGroup("PersonProfile");

            g.MapPut("Create", (IPersonProfileService service, IHttpContextAccessor accessor, PersonProfileCreateRQ rq, CancellationToken cancellationToken) => service.CreateAsync(rq, null, cancellationToken))
                .WithDescription("Person profile create / 创建人员档案").WithTags("PersonProfile");

            g.MapPost("CreateLink", (IPersonProfileService service, PersonProfileLinkCreateRQ rq, CancellationToken cancellationToken) => service.CreateLinkAsync(rq, cancellationToken))
                .WithDescription("Create person profile link / 创建人员档案关联").WithTags("PersonProfile");

            g.MapPost("CreateTask", (IPersonProfileService service, PersonTaskCreateRQ rq, CancellationToken cancellationToken) => service.CreateTaskAsync(rq, cancellationToken))
                .WithDescription("Create person task / 创建人员任务").WithTags("PersonProfile");

            g.MapDelete("DeleteAttachment/{id:long}", (IPersonProfileService service, long id, CancellationToken cancellationToken) => service.DeleteAttachmentAsync(id, cancellationToken))
                .WithDescription("Delete person profile attachment / 删除人员档案附件").WithTags("PersonProfile");

            g.MapDelete("DeleteLink/{id:long}", (IPersonProfileService service, long id, CancellationToken cancellationToken) => service.DeleteLinkAsync(id, cancellationToken))
                .WithDescription("Delete person profile link / 删除人员档案链接").WithTags("PersonProfile");

            g.MapPost("List", (IPersonProfileService service, IHttpContextAccessor accessor, PersonProfileListRQ rq, CancellationToken cancellationToken) => service.ListAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Person profile list / 人员档案列表").WithTags("PersonProfile");

            g.MapPost("Query", (IPersonProfileService service, IHttpContextAccessor accessor, PersonProfileQueryRQ rq, CancellationToken cancellationToken) => service.QueryAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Person profile query / 人员档案查询").WithTags("PersonProfile");

            g.MapGet("Read/{id:long}", (IPersonProfileService service, long id, CancellationToken cancellationToken) => service.ReadAsync(id, cancellationToken))
                .WithDescription("Read person profile / 读取人员档案").WithTags("PersonProfile");

            g.MapGet("ReadInner/{id:long}", (IPersonProfileService service, long id, CancellationToken cancellationToken) => service.ReadInnerAsync(id, cancellationToken))
                .WithDescription("Read person profile for query / 读取人员查询浏览档案").WithTags("PersonProfile");

            g.MapPut("Update", (IPersonProfileService service, IHttpContextAccessor accessor, PersonProfileUpdateRQ rq, CancellationToken cancellationToken) => service.UpdateAsync(rq, cancellationToken))
                .WithDescription("Person profile update / 更新人员档案").WithTags("PersonProfile");

            g.MapPut("UpdateAttachment", (IPersonProfileService service, IHttpContextAccessor accessor, PersonProfileAttachmentUpdateRQ rq, CancellationToken cancellationToken) => service.UpdateAttachmentAsync(rq, cancellationToken))
                .WithDescription("Update person profile attachment / 更新人员档案附件").WithTags("PersonProfile");

            g.MapPut("UpdateLink", (IPersonProfileService service, PersonProfileLinkUpdateRQ rq, CancellationToken cancellationToken) => service.UpdateLinkAsync(rq, cancellationToken))
                .WithDescription("Update person profile link / 更新人员档案链接").WithTags("PersonProfile");

            g.MapGet("UpdateRead/{id:long}", (IPersonProfileService service, IHttpContextAccessor accessor, long id, CancellationToken cancellationToken) => service.UpdateReadAsync(id, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Read person profile for update / 读取人员档案用于更新").WithTags("PersonProfile");

            g.MapPost("UploadFilesAction/{id:long}", (IPersonProfileService service, long id, CancellationToken cancellationToken) => service.UploadFilesActionAsync(id, cancellationToken))
                .WithDescription("Upload files action / 上传文件操作").WithTags("PersonProfile");

            return builder;
        }
    }
}
