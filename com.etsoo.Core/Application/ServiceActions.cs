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
        /// Delete entity
        /// 删除实体
        /// </summary>
        /// <param name="ids">Ids</param>
        /// <returns>Operation result</returns>
        public OperationResult DeleteEntity(T[] ids)
        {
            return Execute(GetDeleteData(ids));
        }

        /// <summary>
        /// Async delete entity
        /// 异步删除实体
        /// </summary>
        /// <param name="ids">Ids</param>
        /// <returns>Operation result</returns>
        public async Task<OperationResult> DeleteEntityAsync(T[] ids)
        {
            return await ExecuteAsync(GetDeleteData(ids));
        }

        /// <summary>
        /// Get view database operation data
        /// 获取浏览的数据库端操作数
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="field">Field</param>
        /// <param name="format">Format</param>
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
        /// Get search database operation data
        /// 获取查询的数据库端操作数
        /// </summary>
        /// <param name="domain">Domain</param>
        /// <param name="model">Parameters</param>
        /// <param name="format">Format</param>
        protected virtual OperationData GetSearchData(string domain, IService<T>.ITiplistDataModel model, DataFormat format)
        {
            // Generate key data
            // Something like search_json_common
            var key = new StringBuilder("search");
            key.Append(GetField(domain));
            key.Append(GetFormat(format));

            // Create operation data
            var data = CreateOperationData(key.ToString());

            // Data format
            data.Format = format;

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
        /// Get view database operation data
        /// 获取浏览的数据库端操作数
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="field">Field</param>
        /// <param name="format">Format</param>
        protected virtual OperationData GetViewData(T id, string field, DataFormat format)
        {
            // Generate key data
            var key = new StringBuilder("view");
            key.Append(GetField(field));
            key.Append(GetFormat(format));

            // Create operation data
            var data = CreateOperationData(key.ToString());

            // Data format
            data.Format = format;

            // Add id parameter
            data.Parameters.Add("id", id);

            // Return
            return data;
        }

        /// <summary>
        /// Search JSON data to stream
        /// 查询JSON数据到流
        /// </summary>
        /// <param name="stream">Stream to write</param>
        /// <param name="domain">Domain, identify as a new saved procedure</param>
        /// <param name="model">Parameters</param>
        public void SearchJson<M>(Stream stream, string domain, M model) where M : IService<T>.ITiplistDataModel
        {
            Execute(stream, GetSearchData(domain, model, DataFormat.Json));
        }

        /// <summary>
        /// Async search JSON data
        /// 异步查询JSON数据
        /// </summary>
        /// <param name="stream">Stream to write</param>
        /// <param name="domain">Domain, identify as a new saved procedure</param>
        /// <param name="model">Parameters</param>
        public async Task SearchJsonAsync<M>(Stream stream, string domain, M model) where M : IService<T>.ITiplistDataModel
        {
            await ExecuteAsync(stream, GetSearchData(domain, model, DataFormat.Json));
        }

        /// <summary>
        /// View JSON data to stream
        /// 浏览JSON数据到流
        /// </summary>
        /// <param name="stream">Stream to write</param>
        /// <param name="id">Id</param>
        /// <param name="field">Field</param>
        public void ViewJson(Stream stream, T id, string field = null)
        {
            Execute(stream, GetViewData(id, field, DataFormat.Json));
        }

        /// <summary>
        /// Async view JSON data
        /// 异步浏览JSON数据
        /// </summary>
        /// <param name="stream">Stream to write</param>
        /// <param name="id">Id</param>
        /// <param name="field">Field</param>
        public async Task ViewJsonAsync(Stream stream, T id, string field = null)
        {
            await ExecuteAsync(stream, GetViewData(id, field, DataFormat.Json));
        }
    }
}