using com.etsoo.Core.Services;
using com.etsoo.SmartERP.Applications;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Threading.Tasks;

namespace com.etsoo.Api.Helpers
{
    /// <summary>
    /// Extended Abstract Common Controller
    /// 扩展的抽象控制器
    /// </summary>
    /// <typeparam name="S">Generic MainApp service</typeparam>
    public abstract class CommonController<S, T> : ControllerBase where T : struct, IComparable where S : IMainService<T>
    {
        /// <summary>
        /// Distributed cache
        /// 分布式缓存
        /// </summary>
        protected IDistributedCache Cache { get; }

        /// <summary>
        /// Service
        /// 服务对象
        /// </summary>
        protected S Service { get; }

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="service">Current service</param>
        /// <param name="distributedCache">Distributed cache</param>
        public CommonController(S service, IDistributedCache distributedCache)
        {
            // Set service
            Service = service;

            // Set cache
            Cache = distributedCache;
        }

        /// <summary>
        /// Common delete entity
        /// 通用删除对象
        /// </summary>
        /// <param name="id">Ids</param>
        [HttpDelete("{id}")]
        public async Task Delete(T id)
        {
            await Delete(new T[] { id });
        }

        /// <summary>
        /// Common delete entity, array supported, ?ids=1&ids=2
        /// 通用删除对象，支持传入数组
        /// </summary>
        /// <param name="ids">Id array</param>
        [HttpDelete]
        public async Task Delete([FromQuery(Name = "ids")] T[] ids)
        {
            await ResultContentAsync(await Service.DeleteEntityAsync(ids));
        }

        /// <summary>
        /// Common view entity
        /// 通用浏览对象
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="field">Field</param>
        /// <returns>JSON view data</returns>
        [HttpGet("{id}/{field?}")]
        public async Task Get(T id, string field = null)
        {
            Response.ContentType = "application/json";
            await Service.ViewJsonAsync(Response.Body, id, field);
        }

        /// <summary>
        /// Data report
        /// 数据报表
        /// </summary>
        /// <param name="id">Field of data</param>
        /// <param name="parameters">Parameters(JSON? depends on SP logic)</param>
        /// <returns>JSON data</returns>
        [HttpGet("Report/{id}")]
        public async Task Report(string id, [FromQuery(Name = "p")] string parameters = null)
        {
            Response.ContentType = "application/json";
            await Service.ReportAsync(Response.Body, id, parameters);
        }

        /// <summary>
        /// Async set operation result content
        /// 异步设置操作结果内容
        /// </summary>
        /// <param name="result">Result</param>
        /// <returns>Task</returns>
        protected async Task ResultContentAsync(OperationResult result)
        {
            Response.ContentType = "application/json";
            await result.SerializeAsync(Response.Body, DataFormat.Json);
        }

        /// <summary>
        /// Async search entities
        /// 异步查询对象
        /// </summary>
        /// <param name="model">Conditions model</param>
        /// <returns>Search result</returns>
        protected async Task SearchEntityAsync<M>(M model) where M : IMainService<T>.ISearchDataModel
        {
            Response.ContentType = "application/json";
            await Service.SearchJsonAsync(Response.Body, model.Domain, model);
        }

        /// <summary>
        /// Async tiplist entities
        /// 异步列表对象
        /// </summary>
        /// <param name="model">Conditions model</param>
        /// <returns>Task</returns>
        protected async Task TiplistAsync<M>(M model) where M : IMainService<T>.ITiplistDataModel
        {
            Response.ContentType = "application/json";
            await Service.SearchJsonAsync(Response.Body, "tiplist", model);
        }
    }
}