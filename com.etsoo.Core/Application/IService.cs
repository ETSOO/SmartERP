using com.etsoo.Core.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace com.etsoo.Core.Application
{
    /// <summary>
    /// Service interface
    /// 服务接口
    /// </summary>
    /// <typeparam name="T">Generic for id type</typeparam>
    public partial interface IService<T> where T : struct, IComparable
    {
        /// <summary>
        /// Appliction
        /// 程序对象
        /// </summary>
        IApplication Application { get; }

        /// <summary>
        /// Service identity
        /// 服务标识
        /// </summary>
        string Identity { get; }

        /// <summary>
        /// Module id
        /// 模块编号
        /// </summary>
        byte ModuleId { get; }

        /// <summary>
        /// Support multiple modules
        /// 是否支持多模块
        /// </summary>
        bool MultipleModule { get; }

        /// <summary>
        /// Current user
        /// 当前用户
        /// </summary>
        ICurrentUser User { get; }

        /// <summary>
        /// Delete entity
        /// 删除实体
        /// </summary>
        /// <param name="ids">Ids</param>
        /// <returns>Operation result</returns>
        OperationResult DeleteEntity(T[] ids);

        /// <summary>
        /// Async delete entity
        /// 异步删除实体
        /// </summary>
        /// <param name="ids">Ids</param>
        /// <returns>Operation result</returns>
        Task<OperationResult> DeleteEntityAsync(T[] ids);

        /// <summary>
        /// Search JSON data to stream
        /// 查询JSON数据到流
        /// </summary>
        /// <param name="stream">Stream to write</param>
        /// <param name="domain">Domain, identify as a new saved procedure</param>
        /// <param name="model">Parameters</param>
        void SearchJson<M>(Stream stream, string domain, M model) where M : ITiplistDataModel;

        /// <summary>
        /// Async search JSON data
        /// 异步查询JSON数据
        /// </summary>
        /// <param name="stream">Stream to write</param>
        /// <param name="domain">Domain, identify as a new saved procedure</param>
        /// <param name="model">Parameters</param>
        Task SearchJsonAsync<M>(Stream stream, string domain, M model) where M : ITiplistDataModel;

        /// <summary>
        /// View JSON data to stream
        /// 浏览JSON数据到流
        /// </summary>
        /// <param name="stream">Stream to write</param>
        /// <param name="id">Id</param>
        /// <param name="field">Field</param>
        void ViewJson(Stream stream, T id, string field = null);

        /// <summary>
        /// Async view JSON data to stream
        /// 异步浏览JSON数据到流
        /// </summary>
        /// <param name="stream">Stream to write</param>
        /// <param name="id">Id</param>
        /// <param name="field">Field</param>
        Task ViewJsonAsync(Stream stream, T id, string field = null);
    }
}