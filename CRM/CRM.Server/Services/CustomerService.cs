using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.ServiceApp.SmartERP;
using CRM.Server.Dto.Customer;
using CRM.Server.RQ.Customer;
using Microsoft.EntityFrameworkCore;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using PlatformShared.Extentions;
using System.Buffers;

namespace CRM.Server.Services
{
    /// <summary>
    /// Customer service
    /// 客户服务
    /// </summary>
    public class CustomerService : SEUserService, ICustomerService
    {
        readonly MyDbContext _db;

        public CustomerService(
            MyDbContext db,
            ISEServiceApp app,
            CurrentUserAccessor userAccessor,
            ILogger<CustomerService> logger
        )
            : base(app, userAccessor.UserSafe, "customer", logger)
        {
            _db = db;
        }

        private IQueryable<Person> CreateQuery(CustomerListRQ rq, Func<IQueryable<Person>, IQueryable<Person>>? filters = null)
        {
            var query = _db.Customers(User.OrganizationInt).AsNoTracking()
                .QueryEtsoo(rq, (c) => c.Id, (c) => c.Status, (q) =>
                {
                    if (rq.Keyword?.Length > 1)
                    {
                        var keyword = rq.Keyword;

                        if (keyword.IsComplexQueryKeywords())
                        {
                            q = q.QueryEtsooKeywords(keyword, DbUtils.ILikeMethod, c => c.Name, c => c.PreferredName);
                        }
                        else
                        {
                            q = q.Where(c => EF.Functions.ILike(c.Name, $"%{keyword}%")
                            || (c.QueryKeyword != null && EF.Functions.ILike(c.QueryKeyword, $"%{keyword}%"))
                            || (c.PreferredName != null && EF.Functions.ILike(c.PreferredName, $"%{keyword}%"))
                            );
                        }
                    }

                    if (filters != null)
                    {
                        q = filters(q);
                    }

                    return q;
                });

            return query;
        }

        /// <summary>
        /// List customer JSON data
        /// 客户列表JSON数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task ListAsync(CustomerListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            await CreateQuery(rq)
                .Select(c => new CustomerListData
                {
                    Id = c.Id,
                    Name = c.Name
                }).ToJsonAsync(writer, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Query customer JSON data
        /// 查询客户JSON数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task QueryAsync(CustomerQueryRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            await CreateQuery(rq)
                .Select(c => new CustomerQueryData
                {
                    Id = c.Id,
                    Name = c.Name
                }).ToJsonAsync(writer, cancellationToken: cancellationToken);
        }
    }
}