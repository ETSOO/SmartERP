using com.etsoo.WebUtils;
using CRM.Server.RQ.PersonContact;
using CRM.Server.Services;

namespace CRM.Server.Endpoints
{
    /// <summary>
    /// Person contact service APIs
    /// 人员联系人服务接口
    /// </summary>
    internal static class PersonContact
    {
        public static RouteGroupBuilder MapPersonContact(this RouteGroupBuilder builder)
        {
            var g = builder.MapGroup("PersonContact");

            g.MapPost("Add", (IPersonContactService service, ContactRelationAddRQ rq, CancellationToken cancellationToken) => service.AddAsync(rq, cancellationToken))
                .WithDescription("Add contact / 添加联系人").WithTags("PersonContact");

            g.MapPost("Create", (IPersonContactService service, ContactCreateRQ rq, CancellationToken cancellationToken) => service.CreateAsync(rq, cancellationToken))
                .WithDescription("Create contact / 创建联系人").WithTags("PersonContact");

            g.MapDelete("Delete/{id:long}", (IPersonContactService service, long id, CancellationToken cancellationToken) => service.DeleteAsync(id, cancellationToken))
                .WithDescription("Delete contact / 删除联系人").WithTags("PersonContact");

            g.MapPost("List", (IPersonContactService service, IHttpContextAccessor accessor, ContactListRQ rq, CancellationToken cancellationToken) => service.ListAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Contact list / 联系人列表").WithTags("PersonContact");

            g.MapPost("Query", (IPersonContactService service, IHttpContextAccessor accessor, ContactQueryRQ rq, CancellationToken cancellationToken) => service.QueryAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Contact query / 联系人查询").WithTags("PersonContact");

            g.MapPut("UpdateRelation", (IPersonContactService service, ContactRelationUpdateRQ rq, CancellationToken cancellationToken) => service.UpdateRelationAsync(rq, cancellationToken))
                .WithDescription("Update contact relation / 更新联系人关系").WithTags("PersonContact");

            g.MapGet("UpdateRelationRead/{id:long}", (IPersonContactService service, long id, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.UpdateRelationReadAsync(id, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Read contact relation update data / 读取联系人关系更新数据").WithTags("PersonContact");

            return builder;
        }
    }
}
