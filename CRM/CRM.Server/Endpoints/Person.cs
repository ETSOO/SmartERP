using com.etsoo.WebUtils;
using CRM.Server.RQ.Person;
using CRM.Server.Services;

namespace CRM.Server.Endpoints
{
    /// <summary>
    /// Person service APIs
    /// 人员服务API
    /// </summary>
    internal static class Person
    {
        public static RouteGroupBuilder MapPerson(this RouteGroupBuilder builder)
        {
            var g = builder.MapGroup("Person");

            g.MapPost("Choose", (IPersonService service, ChoosePersonsRQ rq, CancellationToken cancellationToken) => service.ChoosePersonsAsync(rq, cancellationToken))
                .WithDescription("Person choose / 人员选择").WithTags("Person");

            g.MapPost("CreateAddress", (IPersonService service, AddressCreateRQ rq, CancellationToken cancellationToken) => service.CreateAddressAsync(rq, cancellationToken))
                .WithDescription("Create address / 创建地址").WithTags("Person");

            g.MapPost("CreateInfo", (IPersonService service, PersonInfoCreateRQ rq, CancellationToken cancellationToken) => service.CreateInfoAsync(rq, cancellationToken))
                .WithDescription("Create person info / 创建人员信息").WithTags("Person");

            g.MapDelete("DeleteAddress/{id:int}", (IPersonService service, int id, CancellationToken cancellationToken) => service.DeleteAddressAsync(id, cancellationToken))
                .WithDescription("Delete address / 删除地址").WithTags("Person");

            g.MapDelete("DeleteInfo/{id:int}", (IPersonService service, int id, CancellationToken cancellationToken) => service.DeleteInfoAsync(id, cancellationToken))
                .WithDescription("Delete person info / 删除人员信息").WithTags("Person");

            g.MapPost("List", (IPersonService service, IHttpContextAccessor accessor, PersonListRQ rq, CancellationToken cancellationToken) => service.ListAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Person list / 人员列表").WithTags("Person");

            g.MapPost("Query", (IPersonService service, IHttpContextAccessor accessor, PersonQueryRQ rq, CancellationToken cancellationToken) => service.QueryAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Person query / 人员查询").WithTags("Person");

            g.MapPost("QueryInfo", (IPersonService service, IHttpContextAccessor accessor, PersonInfoQueryRQ rq, CancellationToken cancellationToken) => service.QueryInfoAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Person info query / 人员信息查询").WithTags("Person");

            g.MapGet("Read/{id:long}", (IPersonService service, long id, CancellationToken cancellationToken) => service.ReadAsync(id, cancellationToken))
                .WithDescription("Read person / 读取人员").WithTags("Person");

            g.MapGet("ReadInfo/{id:int}", (IPersonService service, int id, CancellationToken cancellationToken) => service.ReadInfoAsync(id, cancellationToken))
                .WithDescription("Read person info / 读取人员信息").WithTags("Person");

            g.MapPut("Update", (IPersonService service, PersonUpdateRQ rq, CancellationToken cancellationToken) => service.UpdateAsync(rq, cancellationToken))
                .WithDescription("Update person / 更新人员").WithTags("Person");

            g.MapPut("UpdateAddress", (IPersonService service, AddressUpdateRQ rq, CancellationToken cancellationToken) => service.UpdateAddressAsync(rq, cancellationToken))
                .WithDescription("Update address / 更新地址").WithTags("Person");

            g.MapPut("UpdateInfo", (IPersonService service, PersonInfoUpdateRQ rq, CancellationToken cancellationToken) => service.UpdateInfoAsync(rq, cancellationToken))
                .WithDescription("Update person info / 更新人员信息").WithTags("Person");

            g.MapGet("UpdateAddressRead/{id:int}", (IPersonService service, int id, CancellationToken cancellationToken) => service.UpdateAddressReadAsync(id, cancellationToken))
                .WithDescription("Read address update data / 读取地址更新数据").WithTags("Person");

            g.MapGet("UpdateRead/{id:long}", (IPersonService service, long id, CancellationToken cancellationToken) => service.UpdateReadAsync(id, cancellationToken))
                .WithDescription("Read person update data / 读取人员更新数据").WithTags("Person");

            return builder;
        }
    }
}
