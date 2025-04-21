using com.etsoo.WebUtils;
using CRM.Server.RQ.Person;
using CRM.Server.Services;

namespace CRM.Server.Endpoints
{
    /// <summary>
    /// Person service APIs
    /// 人员服务API
    /// </summary>
    public static class Person
    {
        public static RouteGroupBuilder MapPerson(this RouteGroupBuilder builder)
        {
            var g = builder.MapGroup("Person");

            g.MapPost("Choose", (IPersonService service, ChoosePersonsRQ rq, CancellationToken cancellationToken) => service.ChoosePersonsAsync(rq, cancellationToken))
                .WithDescription("Person choose / 人员选择").WithTags("Person");

            g.MapPost("List", (IPersonService service, IHttpContextAccessor accessor, PersonListRQ rq, CancellationToken cancellationToken) => service.ListAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Person list / 人员列表").WithTags("Person");

            g.MapPost("Query", (IPersonService service, IHttpContextAccessor accessor, PersonQueryRQ rq, CancellationToken cancellationToken) => service.QueryAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Person query / 人员查询").WithTags("Person");

            g.MapGet("Read/{id:long}", (IPersonService service, long id, CancellationToken cancellationToken) => service.ReadAsync(id, cancellationToken))
                .WithDescription("Read person / 读取人员").WithTags("Person");

            return builder;
        }
    }
}
