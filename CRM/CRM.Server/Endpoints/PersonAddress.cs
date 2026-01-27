using com.etsoo.WebUtils;
using CRM.Server.RQ.PersonAddress;
using CRM.Server.Services;

namespace CRM.Server.Endpoints
{
    /// <summary>
    /// Person address service APIs
    /// 人员地址服务接口
    /// </summary>
    internal static class PersonAddress
    {
        public static RouteGroupBuilder MapPersonAddress(this RouteGroupBuilder builder)
        {
            var g = builder.MapGroup("PersonAddress");

            g.MapPost("Create", (IPersonAddressService service, AddressCreateRQ rq, CancellationToken cancellationToken) => service.CreateAsync(rq, cancellationToken))
                .WithDescription("Create address / 创建地址").WithTags("PersonAddress");

            g.MapPost("CreateLocation", (IPersonAddressService service, AddressLocationCreateRQ rq, CancellationToken cancellationToken) => service.CreateLocationAsync(rq, cancellationToken))
                .WithDescription("Create address location / 创建地址位置").WithTags("PersonAddress");

            g.MapDelete("Delete/{id:int}", (IPersonAddressService service, int id, CancellationToken cancellationToken) => service.DeleteAsync(id, cancellationToken))
                .WithDescription("Delete address / 删除地址").WithTags("PersonAddress");

            g.MapPost("List", (IPersonAddressService service, AddressListRQ rq, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.ListAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Get person address list / 获取人员地址列表").WithTags("PersonAddress");

            g.MapPost("Query", (IPersonAddressService service, AddressListRQ rq, CancellationToken cancellationToken) => service.QueryAsync(rq, cancellationToken))
                .WithDescription("Query person address / 查询人员地址").WithTags("PersonAddress");

            g.MapPut("Update", (IPersonAddressService service, AddressUpdateRQ rq, CancellationToken cancellationToken) => service.UpdateAsync(rq, cancellationToken))
                .WithDescription("Update address / 更新地址").WithTags("PersonAddress");

            g.MapGet("UpdateRead/{id:int}", (IPersonAddressService service, int id, CancellationToken cancellationToken) => service.UpdateReadAsync(id, cancellationToken))
                .WithDescription("Read address update data / 读取地址更新数据").WithTags("PersonAddress");

            return builder;
        }
    }
}
