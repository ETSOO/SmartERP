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
        /// Add an entity
        /// 添加实体
        /// </summary>
        /// <param name="model">Entity model</param>
        /// <returns>Operation result</returns>
        OperationResult AddEntity(IService<T>.IDataModel model);

        /// <summary>
        /// Async add an entity
        /// 异步添加实体
        /// </summary>
        /// <param name="model">Entity model</param>
        /// <returns>Operation result</returns>
        Task<OperationResult> AddEntityAsync(IService<T>.IDataModel model);

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
        /// Edit an entity
        /// 修改实体
        /// </summary>
        /// <param name="model">Entity model</param>
        /// <returns>Operation result</returns>
        OperationResult EditEntity(IService<T>.IIdDataModel model);

        /// <summary>
        /// Async edit an entity
        /// 异步修改实体
        /// </summary>
        /// <param name="model">Entity model</param>
        /// <returns>Operation result</returns>
        Task<OperationResult> EditEntityAsync(IService<T>.IIdDataModel model);

        /// <summary>
        /// Data report
        /// 数据报表
        /// </summary>
        /// <param name="stream">Stream to write</param>
        /// <param name="id">Field of data</param>
        /// <param name="parameters">Parameters</param>
        /// <returns>Is content wrote</returns>
        bool Report(Stream stream, string id, string parameters = null);

        /// <summary>
        /// Async data report
        /// 异步数据报表
        /// </summary>
        /// <param name="stream">Stream to write</param>
        /// <param name="id">Field of data</param>
        /// <param name="parameters">Parameters</param>
        /// <returns>Is content wrote</returns>
        Task<bool> ReportAsync(Stream stream, string id, string parameters = null);

        /// <summary>
        /// Search JSON data to stream
        /// 查询JSON数据到流
        /// </summary>
        /// <param name="stream">Stream to write</param>
        /// <param name="domain">Domain, identify as a new saved procedure</param>
        /// <param name="model">Parameters</param>
        /// <returns>Is content wrote</returns>
        bool SearchJson<M>(Stream stream, string domain, M model) where M : ITiplistDataModel;

        /// <summary>
        /// Async search JSON data
        /// 异步查询JSON数据
        /// </summary>
        /// <param name="stream">Stream to write</param>
        /// <param name="domain">Domain, identify as a new saved procedure</param>
        /// <param name="model">Parameters</param>
        /// <returns>Is content wrote</returns>
        Task<bool> SearchJsonAsync<M>(Stream stream, string domain, M model) where M : ITiplistDataModel;

        /// <summary>
        /// View JSON data to stream
        /// 浏览JSON数据到流
        /// </summary>
        /// <param name="stream">Stream to write</param>
        /// <param name="id">Id</param>
        /// <param name="field">Field</param>
        /// <returns>Is content wrote</returns>
        bool ViewJson(Stream stream, T id, string field = null);

        /// <summary>
        /// Async view JSON data to stream
        /// 异步浏览JSON数据到流
        /// </summary>
        /// <param name="stream">Stream to write</param>
        /// <param name="id">Id</param>
        /// <param name="field">Field</param>
        /// <returns>Is content wrote</returns>
        Task<bool> ViewJsonAsync(Stream stream, T id, string field = null);
    }
}