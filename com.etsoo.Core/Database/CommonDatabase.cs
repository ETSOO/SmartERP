using com.etsoo.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace com.etsoo.Core.Database
{
    /// <summary>
    /// Abstract Common Database
    /// 通用数据库抽象类
    /// </summary>
    public abstract class CommonDatabase : ICommonDatabase
    {
        /// <summary>
        /// Connection string
        /// 链接字符串
        /// </summary>
        public string ConnectionString { get; }

        /// <summary>
        /// Data parameter parser
        /// 数据参数解析器
        /// </summary>
        public IDataParameterParser DataParameterParser { get; set; }

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="connectionString">Connection string</param>
        public CommonDatabase(string connectionString)
        {
            ConnectionString = connectionString;
        }

        /// <summary>
        /// Add data parameter
        /// 添加数据列表参数
        /// </summary>
        /// <typeparam name="T">Type generic</typeparam>
        /// <param name="items">Data list</param>
        /// <param name="parameters">Operation parameters</param>
        /// <param name="name">Parameter name</param>
        /// <param name="hasRowIndex">Has row index</param>
        public virtual void AddDataParameter<T>(IEnumerable<T> items, Dictionary<string, dynamic> parameters, string name, bool? hasRowIndex) where T : IComparable
        {
            // No further process when empty
            if (items == null || !items.Any())
            {
                return;
            }

            // When hasRowIndex not equal to null, need unique
            if (hasRowIndex != null)
                items = items.Distinct();

            if (DataParameterParser == null)
            {
                // Join the items with comma
                parameters.Add(name, string.Join(',', items));
            }
            else
            {
                // Pass to the defined interface
                DataParameterParser.AddDataParameter(items, parameters, name, hasRowIndex);
            }
        }

        /// <summary>
        /// Create EF Database Context
        /// 创建EF数据库上下文
        /// </summary>
        /// <typeparam name="M">Model class</typeparam>
        /// <returns>Database Context</returns>
        public abstract CommonDbContext<M> CreateContext<M>() where M : class, new();

        /// <summary>
        /// Execute SQL Command, return rows affacted
        /// 执行SQL命令，返回影响的行数
        /// </summary>
        /// <param name="sql">SQL Command</param>
        /// <param name="isStoredProcedure">Is stored procedure</param>
        /// <returns>Rows affacted</returns>
        public int Execute(string sql, bool? isStoredProcedure = false)
        {
            return Execute(sql, null, isStoredProcedure);
        }

        /// <summary>
        /// Async Execute SQL Command
        /// 异步执行SQL命令
        /// </summary>
        /// <param name="sql">SQL Command</param>
        /// <param name="isStoredProcedure">Is stored procedure</param>
        /// <returns>Rows affacted</returns>
        public async Task<int> ExecuteAsync(string sql, bool? isStoredProcedure = false)
        {
            return await ExecuteAsync(sql, null, isStoredProcedure);
        }

        /// <summary>
        /// Execute SQL Command, return rows affacted
        /// 执行SQL命令，返回影响的行数
        /// </summary>
        /// <param name="sql">SQL Command</param>
        /// <param name="paras">Parameters</param>
        /// <param name="isStoredProcedure">Is stored procedure</param>
        /// <returns>Rows affacted</returns>
        public abstract int Execute(string sql, IDictionary<string, dynamic> paras, bool? isStoredProcedure = false);

        /// <summary>
        /// Async Execute SQL Command
        /// 异步执行SQL命令
        /// </summary>
        /// <param name="sql">SQL Command</param>
        /// <param name="paras">Parameters</param>
        /// <param name="isStoredProcedure">Is stored procedure</param>
        /// <returns>Rows affacted</returns>
        public abstract Task<int> ExecuteAsync(string sql, IDictionary<string, dynamic> paras, bool? isStoredProcedure = false);

        /// <summary>
        /// Execute SQL Command to return first row first column value
        /// 执行SQL命令，返回第一行第一列的值
        /// </summary>
        /// <param name="sql">SQL Command</param>
        /// <param name="isStoredProcedure">Is stored procedure</param>
        /// <returns>First row first column's value</returns>
        public object ExecuteScalar(string sql, bool? isStoredProcedure = false)
        {
            return ExecuteScalar(sql, null, isStoredProcedure);
        }

        /// <summary>
        /// Async Execute SQL Command to return first row first column value
        /// 异步执行SQL命令，返回第一行第一列的值
        /// </summary>
        /// <param name="sql">SQL Command</param>
        /// <param name="isStoredProcedure">Is stored procedure</param>
        /// <returns>First row first column's value</returns>
        public async Task<object> ExecuteScalarAsync(string sql, bool? isStoredProcedure = false)
        {
            return await ExecuteScalarAsync(sql, null, isStoredProcedure);
        }

        /// <summary>
        /// Execute SQL Command to return first row first column value
        /// 执行SQL命令，返回第一行第一列的值
        /// </summary>
        /// <param name="sql">SQL Command</param>
        /// <param name="paras">Parameters</param>
        /// <param name="isStoredProcedure">Is stored procedure</param>
        /// <returns>First row first column's value</returns>
        public abstract object ExecuteScalar(string sql, IDictionary<string, dynamic> paras, bool? isStoredProcedure = false);

        /// <summary>
        /// Async Execute SQL Command to return first row first column value
        /// 异步执行SQL命令，返回第一行第一列的值
        /// </summary>
        /// <param name="sql">SQL Command</param>
        /// <param name="paras">Parameters</param>
        /// <param name="isStoredProcedure">Is stored procedure</param>
        /// <returns>First row first column's value</returns>
        public abstract Task<object> ExecuteScalarAsync(string sql, IDictionary<string, dynamic> paras, bool? isStoredProcedure = false);

        /// <summary>
        /// Execute SQL Command to return operation result
        /// 执行SQL命令，返回操作结果对象
        /// </summary>
        /// <param name="sql">SQL Command</param>
        /// <param name="isStoredProcedure">Is stored procedure</param>
        /// <returns>Operation result</returns>
        public OperationResult ExecuteResult(string sql, bool? isStoredProcedure = false)
        {
            return ExecuteResult(sql, null, isStoredProcedure);
        }

        /// <summary>
        /// Async execute SQL Command to return operation result
        /// 异步执行SQL命令，返回操作结果对象
        /// </summary>
        /// <param name="sql">SQL Command</param>
        /// <param name="isStoredProcedure">Is stored procedure</param>
        /// <returns>Operation result</returns>
        public async Task<OperationResult> ExecuteResultAsync(string sql, bool? isStoredProcedure = false)
        {
            return await ExecuteResultAsync(sql, null, isStoredProcedure);
        }

        /// <summary>
        /// Execute SQL Command to return operation result
        /// 执行SQL命令，返回操作结果对象
        /// </summary>
        /// <param name="sql">SQL Command</param>
        /// <param name="paras">Parameters</param>
        /// <param name="isStoredProcedure">Is stored procedure</param>
        /// <returns>Operation result</returns>
        public abstract OperationResult ExecuteResult(string sql, IDictionary<string, dynamic> paras, bool? isStoredProcedure = false);

        /// <summary>
        /// Async execute SQL Command to return operation result
        /// 异步执行SQL命令，返回操作结果对象
        /// </summary>
        /// <param name="sql">SQL Command</param>
        /// <param name="paras">Parameters</param>
        /// <param name="isStoredProcedure">Is stored procedure</param>
        /// <returns>Operation result</returns>
        public abstract Task<OperationResult> ExecuteResultAsync(string sql, IDictionary<string, dynamic> paras, bool? isStoredProcedure = false);

        /// <summary>
        /// Execute SQL Command, write to stream of the first row first column value, used to read huge text data like json/xml
        /// 执行SQL命令，读取第一行第一列的数据到流，用于读取大文本字段，比如返回的JSON/XML数据
        /// </summary>
        /// <param name="stream">Stream to write</param>
        /// <param name="sql">SQL Command</param>
        /// <param name="isStoredProcedure">Is stored procedure</param>
        /// <returns>Is content wrote</returns>
        public bool ExecuteToStream(Stream stream, string sql, bool? isStoredProcedure = false)
        {
            return ExecuteToStream(stream, sql, null, isStoredProcedure);
        }

        /// <summary>
        /// Async Execute SQL Command, write to stream of the first row first column value, used to read huge text data like json/xml
        /// 异步执行SQL命令，读取第一行第一列的数据到流，用于读取大文本字段，比如返回的JSON/XML数据
        /// </summary>
        /// <param name="stream">Stream to write</param>
        /// <param name="sql">SQL Command</param>
        /// <param name="isStoredProcedure">Is stored procedure</param>
        /// <returns>Is content wrote</returns>
        public async Task<bool> ExecuteToStreamAsync(Stream stream, string sql, bool? isStoredProcedure = false)
        {
            return await ExecuteToStreamAsync(stream, sql, null, isStoredProcedure);
        }

        /// <summary>
        /// Execute SQL Command, write to stream of the first row first column value, used to read huge text data like json/xml
        /// 执行SQL命令，读取第一行第一列的数据到流，用于读取大文本字段，比如返回的JSON/XML数据
        /// </summary>
        /// <param name="stream">Stream to write</param>
        /// <param name="sql">SQL Command</param>
        /// <param name="paras">Parameters</param>
        /// <param name="isStoredProcedure">Is stored procedure</param>
        /// <returns>Is content wrote</returns>
        public abstract bool ExecuteToStream(Stream stream, string sql, IDictionary<string, dynamic> paras, bool? isStoredProcedure = false);

        /// <summary>
        /// Async ESQL Command, write to stream of the first row first column value, used to read huge text data like json/xml
        /// 异步执行SQL命令，读取第一行第一列的数据到流，用于读取大文本字段，比如返回的JSON/XML数据
        /// </summary>
        /// <param name="stream">Stream to write</param>
        /// <param name="sql">SQL Command</param>
        /// <param name="paras">Parameters</param>
        /// <param name="isStoredProcedure">Is stored procedure</param>
        /// <returns>Is content wrote</returns>
        public abstract Task<bool> ExecuteToStreamAsync(Stream stream, string sql, IDictionary<string, dynamic> paras, bool? isStoredProcedure = false);
    }
}