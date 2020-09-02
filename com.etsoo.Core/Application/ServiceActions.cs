using com.etsoo.Core.Services;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace com.etsoo.Core.Application
{
    /// <summary>
    /// Common service, actions
    /// 通用服务，操作汇总
    /// </summary>
    /// <typeparam name="T">Id type generic</typeparam>
    public abstract partial class Service<T> : IService<T> where T : struct, IComparable
    {
        /// <summary>
        /// Add an entity
        /// 添加实体
        /// </summary>
        /// <param name="model">Entity model</param>
        /// <returns>Operation result</returns>
        public OperationResult AddEntity(IService<T>.IDataModel model)
        {
            return Execute(GetAddEntityData(model));
        }

        /// <summary>
        /// Async add an entity
        /// 异步添加实体
        /// </summary>
        /// <param name="model">Entity model</param>
        /// <returns>Operation result</returns>
        public async Task<OperationResult> AddEntityAsync(IService<T>.IDataModel model)
        {
            return await ExecuteAsync(GetAddEntityData(model));
        }

        /// <summary>
        /// Delete entity
        /// 删除实体
        /// </summary>
        /// <param name="ids">Ids</param>
        /// <returns>Operation data</returns>
        public OperationResult DeleteEntity(T[] ids)
        {
            return Execute(GetDeleteData(ids));
        }

        /// <summary>
        /// Async delete entity
        /// 异步删除实体
        /// </summary>
        /// <param name="ids">Ids</param>
        /// <returns>Operation data</returns>
        public async Task<OperationResult> DeleteEntityAsync(T[] ids)
        {
            return await ExecuteAsync(GetDeleteData(ids));
        }

        /// <summary>
        /// Edit an entity
        /// 修改实体
        /// </summary>
        /// <param name="model">Entity model</param>
        /// <returns>Operation result</returns>
        public OperationResult EditEntity(IService<T>.IIdDataModel model)
        {
            return Execute(GetEditEntityData(model));
        }

        /// <summary>
        /// Async edit an entity
        /// 异步修改实体
        /// </summary>
        /// <param name="model">Entity model</param>
        /// <returns>Operation result</returns>
        public async Task<OperationResult> EditEntityAsync(IService<T>.IIdDataModel model)
        {
            return await ExecuteAsync(GetEditEntityData(model));
        }

        /// <summary>
        /// Get add entity database operation data
        /// 获取添加实体的数据库端操作数
        /// </summary>
        /// <param name="model">Entity model</param>
        /// <returns>Operation data</returns>
        protected virtual OperationData GetAddEntityData(IService<T>.IDataModel model)
        {
            // Create operation data
            var data = CreateOperationData("add");

            // Validate model
            if (model == null)
            {
                // No entity model
                data.TestResult.SetError(-1, "model", "No entity data", "no_data");
            }
            else if (data.TestResult.Update(ValidateModel(model)))
            {
                // add parameters
                model.Parameterize(data, this);
            }

            // Return
            return data;
        }

        /// <summary>
        /// Get view database operation data
        /// 获取浏览的数据库端操作数
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="field">Field</param>
        /// <param name="format">Format</param>
        /// <returns>Operation data</returns>
        protected virtual OperationData GetDeleteData(T[] ids)
        {
            // Create operation data
            var data = CreateOperationData("deletes");

            // Add ids parameter
            Application.Database.AddDataParameter(ids, data.Parameters, "ids", false);

            // Return
            return data;
        }

        /// <summary>
        /// Get edit entity database operation data
        /// 获取修改实体的数据库端操作数
        /// </summary>
        /// <param name="model">Entity model</param>
        /// <returns>Operation data</returns>
        protected virtual OperationData GetEditEntityData(IService<T>.IIdDataModel model)
        {
            // Create operation data
            var data = CreateOperationData("edit");

            // Validate model
            if (model == null)
            {
                // No entity model
                data.TestResult.SetError(-1, "model", "No entity data", "no_data");
            }
            else if (data.TestResult.Update(ValidateModel(model)))
            {
                // add parameters
                model.Parameterize(data, this);
            }

            // Return
            return data;
        }

        /// <summary>
        /// Get data report operation data
        /// 获取数据报表操作数
        /// </summary>
        /// <param name="id">Field of data</param>
        /// <param name="parameters">Parameters data</param>
        /// <param name="format">Format</param>
        /// <returns>Operation data</returns>
        protected virtual OperationData GetReportData(string id, string parameters, DataFormat format)
        {
            // Create operation data
            var data = CreateFormatOperationData("report", id, format);

            // Add parameters
            if (!string.IsNullOrEmpty(parameters))
                data.Parameters.Add("parameters", parameters);

            // Return
            return data;
        }

        /// <summary>
        /// Get search database operation data
        /// 获取查询的数据库端操作数
        /// </summary>
        /// <param name="domain">Domain</param>
        /// <param name="model">Parameters</param>
        /// <param name="format">Format</param>
        /// <returns>Operation data</returns>
        protected virtual OperationData GetSearchData(string domain, IService<T>.ITiplistDataModel model, DataFormat format)
        {
            // Create operation data
            var data = CreateFormatOperationData("search", domain, format);

            // Validate model
            if (model != null && data.TestResult.Update(ValidateModel(model)))
            {
                // add parameters
                model.Parameterize(data, this);
            }

            // Return
            return data;
        }

        /// <summary>
        /// Get service summary data
        /// 获取浏览服务汇总操作数
        /// </summary>
        /// <param name="id">Field of data</param>
        /// <param name="format">Format</param>
        /// <returns>Operation data</returns>
        protected virtual OperationData GetServiceSummaryData(string id, DataFormat format)
        {
            // Create operation data
            return CreateFormatOperationData("service_summary", id, format);
        }

        /// <summary>
        /// Get view database operation data
        /// 获取浏览的数据库端操作数
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="field">Field</param>
        /// <param name="format">Format</param>
        /// <returns>Operation data</returns>
        protected virtual OperationData GetViewData(T id, string field, DataFormat format)
        {
            // Create operation data
            var data = CreateFormatOperationData("view", field, format);

            // Add id parameter
            data.Parameters.Add("id", id);

            // Return
            return data;
        }

        /// <summary>
        /// Data report
        /// 数据报表
        /// </summary>
        /// <param name="stream">Stream to write</param>
        /// <param name="id">Field of data</param>
        /// <param name="parameters">Parameters</param>
        /// <returns>Is content wrote</returns>
        public bool Report(Stream stream, string id, string parameters = null)
        {
            return Execute(stream, GetReportData(id, parameters, DataFormat.Json));
        }

        /// <summary>
        /// Async data report
        /// 异步数据报表
        /// </summary>
        /// <param name="stream">Stream to write</param>
        /// <param name="id">Field of data</param>
        /// <param name="parameters">Parameters</param>
        /// <returns>Is content wrote</returns>
        public async Task<bool> ReportAsync(Stream stream, string id, string parameters = null)
        {
            return await ExecuteAsync(stream, GetReportData(id, parameters, DataFormat.Json));
        }

        /// <summary>
        /// Search JSON data to stream
        /// 查询JSON数据到流
        /// </summary>
        /// <param name="stream">Stream to write</param>
        /// <param name="domain">Domain, identify as a new saved procedure</param>
        /// <param name="model">Parameters</param>
        /// <returns>Is content wrote</returns>
        public bool SearchJson<M>(Stream stream, string domain, M model) where M : IService<T>.ITiplistDataModel
        {
            return Execute(stream, GetSearchData(domain, model, DataFormat.Json));
        }

        /// <summary>
        /// Async search JSON data
        /// 异步查询JSON数据
        /// </summary>
        /// <param name="stream">Stream to write</param>
        /// <param name="domain">Domain, identify as a new saved procedure</param>
        /// <param name="model">Parameters</param>
        /// <returns>Is content wrote</returns>
        public async Task<bool> SearchJsonAsync<M>(Stream stream, string domain, M model) where M : IService<T>.ITiplistDataModel
        {
            return await ExecuteAsync(stream, GetSearchData(domain, model, DataFormat.Json));
        }

        /// <summary>
        /// View JSON data to stream
        /// 浏览JSON数据到流
        /// </summary>
        /// <param name="stream">Stream to write</param>
        /// <param name="id">Id</param>
        /// <param name="field">Field</param>
        public bool ViewJson(Stream stream, T id, string field = null)
        {
            return Execute(stream, GetViewData(id, field, DataFormat.Json));
        }

        /// <summary>
        /// Async view JSON data
        /// 异步浏览JSON数据
        /// </summary>
        /// <param name="stream">Stream to write</param>
        /// <param name="id">Id</param>
        /// <param name="field">Field</param>
        /// <returns>Is content wrote</returns>
        public async Task<bool> ViewJsonAsync(Stream stream, T id, string field = null)
        {
            return await ExecuteAsync(stream, GetViewData(id, field, DataFormat.Json));
        }
    }
}