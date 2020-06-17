using com.etsoo.Core.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace com.etsoo.Core.Application
{
    /// <summary>
    /// Common service
    /// 通用服务
    /// </summary>
    /// <typeparam name="T">Id type generic</typeparam>
    public abstract partial class Service<T> : IService<T> where T : struct, IComparable
    {
        /// <summary>
        /// Appliction
        /// 程序对象
        /// </summary>
        public IApplication Application { get; protected set; }

        /// <summary>
        /// Is online check cached
        /// 是否在线探测缓存
        /// </summary>
        public bool Cached { get; protected set; }

        /// <summary>
        /// Service identity
        /// 服务标识
        /// </summary>
        public abstract string Identity { get; }

        /// <summary>
        /// Module id
        /// 模块编号
        /// </summary>
        public abstract byte ModuleId { get; }

        /// <summary>
        /// Support multiple modules
        /// 是否支持多模块
        /// </summary>
        public abstract bool MultipleModule { get; }

        /// <summary>
        /// Current user
        /// 当前用户
        /// </summary>
        public ICurrentUser User { get; protected set; }

        /// <summary>
        /// Create database operation data
        /// 创建数据库操作数
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Operation data</returns>
        protected virtual OperationData CreateOperationData(string key)
        {
            // Define
            var data = new OperationData();

            // Concat the procedure name
            var p = new StringBuilder(Application.Configuration.Flag);
            p.Append("p_");
            p.Append(this.Identity);
            p.Append("_");
            p.Append(key);

            // Set procedure name
            data.Procedure = p.ToString();

            // Return
            return data;
        }

        /// <summary>
        /// Execute database operation result
        /// 执行数据库操作结果
        /// </summary>
        /// <param name="data">Database operation data</param>
        /// <param name="checkPermission">Whether to check permission</param>
        /// <returns>Result</returns>
        protected OperationResult Execute(OperationData data, bool checkPermission = true)
        {
            if (!data.TestResult.OK)
                return data.TestResult;

            if (checkPermission)
            {
                var result = SystemCheck(data);
                if (!result.OK)
                {
                    return result;
                }
            }

            return Application.Database.ExecuteResult(data.Procedure, data.Parameters, true);
        }

        /// <summary>
        /// Execute database operation result to stream
        /// 执行数据库操作结果到流
        /// </summary>
        /// <param name="data">Database operation data</param>
        /// <param name="checkPermission">Whether to check permission</param>
        /// <returns>Result data</returns>
        protected void Execute(Stream stream, OperationData data, bool checkPermission = true)
        {
            if (!data.TestResult.OK)
            {
                data.TestResult.Serialize(stream, data.Format);
                return;
            }

            if (checkPermission)
            {
                var result = SystemCheck(data);
                if (!result.OK)
                {
                    // Not pass the system check, write the result to stream
                    // 如果没有通过系统检查，结果写入到流
                    result.Serialize(stream, data.Format);

                    // Return anyway
                    return;
                }
            }

            Application.Database.ExecuteToStream(stream, data.Procedure, data.Parameters, true);
        }

        /// <summary>
        /// Async execute database operation result
        /// 异步执行数据库操作结果
        /// </summary>
        /// <param name="data">Database operation data</param>
        /// <param name="checkPermission">Whether to check permission</param>
        /// <returns>Result</returns>
        protected async Task<OperationResult> ExecuteAsync(OperationData data, bool checkPermission = true)
        {
            if (!data.TestResult.OK)
                return data.TestResult;

            if (checkPermission)
            {
                var result = SystemCheck(data);
                if (!result.OK)
                {
                    return result;
                }
            }

            return await Application.Database.ExecuteResultAsync(data.Procedure, data.Parameters, true);
        }

        /// <summary>
        /// Async execute database operation result to stream
        /// 异步执行数据库操作结果到流
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="data">Database operation data</param>
        /// <param name="checkPermission">Whether to check permission</param>
        /// <returns>Result data</returns>
        protected async Task ExecuteAsync(Stream stream, OperationData data, bool checkPermission = true)
        {
            if (!data.TestResult.OK)
            {
                await data.TestResult.SerializeAsync(stream, data.Format);
                return;
            }

            if (checkPermission)
            {
                var result = SystemCheck(data);
                if (!result.OK)
                {
                    // Not pass the system check, write the result to stream
                    // 如果没有通过系统检查，结果写入到流
                    await result.SerializeAsync(stream, data.Format);

                    // Return anyway
                    return;
                }
            }

            await Application.Database.ExecuteToStreamAsync(stream, data.Procedure, data.Parameters, true);
        }

        // 格式化字段
        private string GetField(string field)
        {
            if (!string.IsNullOrEmpty(field))
            {
                // Make sure the field is valid to avoid any SQL injection risk
                if (field.Length > 30 || !Regex.IsMatch(field, "^[0-9a-z]+$", RegexOptions.Compiled))
                {
                    throw new ArgumentException(field, nameof(field));
                }

                // Concat field data
                return "_" + field;
            }
            return string.Empty;
        }

        /// <summary>
        /// Get view data format string
        /// 获取浏览数据格式字符串
        /// </summary>
        /// <param name="format">Format</param>
        /// <returns>String represents format part</returns>
        protected virtual string GetFormat(DataFormat format)
        {
            if (format == DataFormat.Default)
                return string.Empty;
            else
                return "_" + format.ToString().ToLower();
        }

        /// <summary>
        /// System check for database operation data
        /// 对数据库操作数的系统检查
        /// </summary>
        /// <param name="data">Operation data</param>
        /// <returns>Result</returns>
        protected abstract OperationResult SystemCheck(OperationData data);

        /// <summary>
        /// Validate model
        /// 验证模型
        /// </summary>
        /// <param name="model">Model</param>
        /// <param name="force">Force to validate</param>
        /// <returns>Result</returns>
        protected OperationResult ValidateModel(IService<T>.IDataModel model, bool force = false)
        {
            // Setup
            var result = new OperationResult();

            // Model name
            var modelName = nameof(model);

            if (model == null)
            {
                // Null exception data
                var nullException = new ArgumentNullException(modelName);
                result.SetError(-1, nullException.ParamName, nullException.Message, "exception");
                return result;
            }

            // Model custom validation
            model.Validate(result);

            // Application validation status
            if (Application.Configuration.ModelValidated && !force)
                return result;

            // Validate
            var context = new ValidationContext(model);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(model, context, validationResults, true))
            {
                // Set validation error
                result.SetError(-1, "model", $"{modelName} validation error!");

                // Parse to Errors
                foreach (var validationResult in validationResults)
                {
                    if (!validationResult.MemberNames.Any())
                    {
                        result.Errors.Add("default", validationResult.ErrorMessage);
                        continue;
                    }

                    foreach (var memberName in validationResult.MemberNames)
                    {
                        result.Errors.Add(memberName, validationResult.ErrorMessage);
                    }
                }
            }

            // Return
            return result;
        }
    }
}