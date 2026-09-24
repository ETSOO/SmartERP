using com.etsoo.WebUtils;
using CRM.Server.RQ.FinanceAccount;
using CRM.Server.Services;

namespace CRM.Server.Endpoints
{
    /// <summary>
    /// Finance account service APIs
    /// 财务账户服务API
    /// </summary>
    internal static class FinanceAccount
    {
        public static RouteGroupBuilder MapFinanceAccount(this RouteGroupBuilder builder)
        {
            var g = builder.MapGroup("FinanceAccount");

            g.MapPost("CreateBulk", (IFinanceAccountService service, FinanceAccountCreateBulkRQ rq, CancellationToken cancellationToken) => service.CreateBulkAsync(rq, cancellationToken))
                .WithDescription("Create finance accounts in bulk / 批量创建财务账户").WithTags("FinanceAccount");

            g.MapPost("Create", (IFinanceAccountService service, FinanceAccountCreateRQ rq, CancellationToken cancellationToken) => service.CreateAsync(rq, cancellationToken))
                .WithDescription("Create finance account / 创建财务账户").WithTags("FinanceAccount");

            g.MapPost("CreateCash", (IFinanceAccountService service, FinanceAccountCreateCashRQ rq, CancellationToken cancellationToken) => service.CreateCashAsync(rq, cancellationToken))
                .WithDescription("Create cash finance account / 创建现金财务账户").WithTags("FinanceAccount");

            g.MapDelete("Delete/{id:int}", (IFinanceAccountService service, int id, CancellationToken cancellationToken) => service.DeleteAsync(id, cancellationToken))
                .WithDescription("Delete finance account / 删除财务账户").WithTags("FinanceAccount");

            g.MapPost("List", (IFinanceAccountService service, FinanceAccountListRQ rq, CancellationToken cancellationToken) => service.ListAsync(rq, cancellationToken))
                .WithDescription("List finance accounts / 列出财务账户").WithTags("FinanceAccount");

            g.MapPost("Query", (IFinanceAccountService service, FinanceAccountQueryRQ rq, CancellationToken cancellationToken) => service.QueryAsync(rq, cancellationToken))
                .WithDescription("Query finance accounts / 查询财务账户").WithTags("FinanceAccount");

            g.MapPut("Update", (IFinanceAccountService service, FinanceAccountUpdateRQ rq, CancellationToken cancellationToken) => service.UpdateAsync(rq, cancellationToken))
                .WithDescription("Update finance account / 更新财务账户").WithTags("FinanceAccount");

            g.MapGet("UpdateRead/{id:int}", (IFinanceAccountService service, int id, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.UpdateReadAsync(id, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Read finance account data for update / 读取用于更新的财务账户数据").WithTags("FinanceAccount");

            return builder;
        }
    }
}
