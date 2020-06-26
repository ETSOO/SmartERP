using com.etsoo.Api.Helpers;
using com.etsoo.SmartERP.Applications;
using com.etsoo.SmartERP.Customer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Threading.Tasks;

namespace com.etsoo.Api.Controllers
{
    /// <summary>
    /// Customer controller
    /// 客户控制器
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class CustomerController : CommonController<CustomerSerivce, int>
    {
        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="distributedCache">Distributed cache</param>
        /// <param name="httpContextAccessor">Http context accessor</param>
        public CustomerController(MainApp app, IDistributedCache distributedCache, IHttpContextAccessor httpContextAccessor)
            : base(CustomerSerivce.Create(app, IdentifyUser.Create(httpContextAccessor.HttpContext.User, httpContextAccessor.HttpContext.Connection.RemoteIpAddress)), distributedCache)
        {
            
        }

        /// <summary>
        /// Search customer
        /// 查询客户
        /// </summary>
        /// <param name="field">Field</param>
        /// <returns>Json search result</returns>
        [HttpGet]
        public async Task Get([FromQuery]SearchModel model)
        {
            await SearchEntityAsync(model);
        }

        /// <summary>
        /// Search customer tip list
        /// 查询客户列表
        /// </summary>
        /// <returns>Json search result</returns>
        [HttpGet("Tiplist")]
        public async Task Tiplist([FromQuery]TiplistModel model)
        {
            await TiplistAsync(model);
        }

        // POST: api/Customer
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT: api/Customer/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }
    }
}
