using com.etsoo.ApiModel.Dto.SmartERP.MessageQueue;
using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Authentication;
using com.etsoo.CoreFramework.Models;
using com.etsoo.WebUtils;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Platform.Server.Dto.Org;
using Platform.Server.Endpoints.Org.RQ;
using Platform.Server.Services;

namespace Platform.Server.Endpoints.Org
{
    /// <summary>
    /// Organization service APIs
    /// 机构服务API
    /// </summary>
    public static class Org
    {
        public static RouteGroupBuilder MapOrg(this RouteGroupBuilder builder)
        {
            var g = builder.MapGroup("Org");

            g.MapPut("Create", (IOrgService service, OrgCreateRQ rq, CancellationToken cancellationToken) => service.CreateAsync(rq, cancellationToken))
                .WithDescription("Create organization / 创建新机构").WithTags("Org");

            g.MapPost("CreateApi", [Roles(Constants.AdminRoles)] (IOrgService service, OrgCreateApiRQ rq, CancellationToken cancellationToken) => service.CreateApiAsync(rq, cancellationToken))
                .WithDescription("Create API / 创建接口").WithTags("Org");

            g.MapPost("CreateResource", [Roles(Constants.AdminRoles)] (IOrgService service, OrgCreateResourceRQ rq, CancellationToken cancellationToken) => service.CreateResourceAsync(rq, cancellationToken))
                .WithDescription("Create custom resource / 创建自定义资源").WithTags("Org");

            g.MapDelete("Delete/{id:int}", [Roles(UserRole.Founder)] (IOrgService service, int id, CancellationToken cancellationToken) => service.DeleteAsync(id, cancellationToken))
                .WithDescription("Delete organization / 删除机构").WithTags("Org");

            g.MapGet("DownloadProfileFile/{id:long}", (IOrgService service, long id, CancellationToken cancellationToken) => service.DownloadFileAsync(OrgDownloadKind.Profile, id, cancellationToken))
                .WithDescription("Download profile attachment / 下载档案附件").WithTags("Org");

            g.MapPost("FormatHtmlContent", async (IOrgService service, IHttpContextAccessor accessor, CancellationToken cancellationToken) =>
            {
                // [FromBody] way only works for JSON content type, not plain text
                var content = await accessor.GetBodyAsync(cancellationToken: cancellationToken);
                if (string.IsNullOrEmpty(content)) return null;
                return await service.FormatHtmlContentAsync(content, cancellationToken);
            }).WithDescription("Format HTML content / 格式化HTML内容").WithTags("Org");

            g.MapGet("GetCustomResources/{culture}", (IOrgService service, string culture, CancellationToken cancellationToken) => service.GetCustomResourcesAsync(culture, cancellationToken))
                .WithDescription("Get current organization's custom resources / 获取当前机构的自定义资源").WithTags("Org");

            g.MapPost("GetMy", (IOrgService service, OrgGetMyRQ rq, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.GetMyAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Get my organizations JSON data / 获取我的机构JSON数据").WithTags("Org");

            g.MapPost("Leave/{id:int}", (IOrgService service, int id, CancellationToken cancellationToken) => service.LeaveAsync(id, cancellationToken))
                .WithDescription("Leave organization / 离开机构").WithTags("Org");

            g.MapPost("List", (IOrgService service, OrgListRQ rq, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.ListAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("List organizations JSON data / 列出机构JSON数据").WithTags("Org");

            g.MapPost("Query", (IOrgService service, OrgQueryRQ rq, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.QueryAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Query organizations JSON data / 查询机构JSON数据").WithTags("Org");

            g.MapPost("QueryApi", (IOrgService service, OrgQueryApiRQ rq, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.QueryApiAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Query APIs JSON data / 查询接口JSON数据").WithTags("Org");

            g.MapPost("QueryResource", (IOrgService service, OrgQueryResourceRQ rq, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.QueryResourceAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Query custom resources JSON data / 查询自定义资源JSON数据").WithTags("Org");

            g.MapGet("Read/{id:int}", (IOrgService service, int id, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.ReadAsync(id, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Read organization JSON data / 浏览机构JSON数据").WithTags("Org");

            g.MapGet("RequestToken", (IAntiforgery forgeryService, IHttpContextAccessor accessor) =>
            {
                // Create the token
                var token = forgeryService.GetAndStoreTokens(accessor.HttpContext!);

                // Return the token
                return new AntiforgeryRequestToken { Name = token.FormFieldName, HeaderName = token.HeaderName, Value = token.RequestToken };
            }).WithDescription("Get Antiforgery request token / 获取反伪造请求令牌").WithTags("Org");

            g.MapPost("SendEmail", (IOrgService service, SendEmailMessage message, CancellationToken cancellationToken) => service.SendEmailAsync(message, cancellationToken))
                .WithDescription("Send email / 发送邮件").WithTags("Org");

            g.MapPost("SendSMS", (IOrgService service, SendSMSMessage message, CancellationToken cancellationToken) => service.SendSMSAsync(message, cancellationToken))
                .WithDescription("Send SMS / 发送短信").WithTags("Org");

            g.MapPost("SendProfileEmail", (IOrgService service, SendProfileEmailRQ rq, CancellationToken cancellationToken) => service.SendProfileEmailAsync(rq, cancellationToken))
                .WithDescription("Send profile email / 发送档案邮件").WithTags("Org");

            g.MapPut("Update", [Roles(Constants.AdminRoles)] (IOrgService service, OrgUpdateRQ rq, CancellationToken cancellationToken) => service.UpdateAsync(rq, cancellationToken))
                .WithDescription("Update organization / 更新机构").WithTags("Org");

            g.MapPut("UpdateApi", [Roles(Constants.AdminRoles)] (IOrgService service, OrgUpdateApiRQ rq, CancellationToken cancellationToken) => service.UpdateApiAsync(rq, cancellationToken))
                .WithDescription("Update API / 更新接口").WithTags("Org");

            g.MapPut("UpdateAvatar/{id:int}", [Roles(UserRole.Founder)] (IOrgService service, [FromRoute] int id, [FromForm] IFormFile avatar, CancellationToken cancellationToken) => service.UpdateAvatarAsync(id, avatar.OpenReadStream(), avatar.ContentType, cancellationToken))
                .DisableAntiforgery()
                .WithDescription("Update organization avatar / 更新机构头像").WithTags("Org");

            g.MapGet("UpdateRead/{id:int}", [Roles(Constants.AdminRoles)] (IOrgService service, int id, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.UpdateReadAsync(id, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Read JSON data for upate / 浏览JSON数据用于更新").WithTags("Org");

            g.MapGet("UpdateApiRead/{id:int}", [Roles(Constants.AdminRoles)] (IOrgService service, int id, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.UpdateApiReadAsync(id, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Read JSON data for upate API / 浏览JSON数据用于更新API").WithTags("Org");

            g.MapPost("UploadProfileFiles/{id:long}", (IOrgService service, long id, IFormFileCollection files, CancellationToken cancellationToken) => service.UploadProfileFilesAsync(id, files, cancellationToken))
                .DisableAntiforgery()
                .WithDescription("Upload profile attachments / 上传档案附件").WithTags("Org");

            g.MapGet("UpdateResourceRead/{id:int}", [Roles(Constants.AdminRoles)] (IOrgService service, int id, CancellationToken cancellationToken) => service.UpdateResourceReadAsync(id, cancellationToken))
                .WithDescription("Read JSON data for upate resource / 浏览JSON数据用于更新资源").WithTags("Org");

            return builder;
        }
    }
}
