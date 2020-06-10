using com.etsoo.Core.Services;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.etsoo.Core.Database
{
    /// <summary>
    /// SQLite Database
    /// SQLite 数据库
    /// </summary>
    public class SqliteDatabase : CommonDatabase
    {
        /// <summary>
        /// New connection
        /// 新链接对象
        /// </summary>
        public SqliteConnection NewConnection
        {
            get { return new SqliteConnection(ConnectionString); }
        }

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="connectionString">Database connection string</param>
        public SqliteDatabase(string connectionString) : base(connectionString)
        {

        }

        /// <summary>
        /// Add parameters to command
        /// DBNull.Value for non-empty NULL
        /// 给命令添加参数
        /// </summary>
        /// <param name="command">Command</param>
        /// <param name="paras">Parameters</param>
        public void AddParameters(SqliteCommand command, IDictionary<string, dynamic> paras)
        {
            if (paras == null)
                return;

            command.Parameters.AddRange(paras.Where(item => item.Value != null).Select(item =>
            {
                if (item.Value is SqliteParameter p)
                    return p;
                else
                    return new SqliteParameter(item.Key, item.Value);
            }));
        }

        /// <summary>
        /// Create EF Database Context
        /// 创建EF数据库上下文
        /// </summary>
        /// <typeparam name="M">Model class</typeparam>
        /// <returns>Database Context</returns>
        public override CommonDbContext<M> CreateContext<M>()
        {
            return new SqliteDbContext<M>(ConnectionString);
        }

        /// <summary>
        /// Execute SQL Command, return rows affacted
        /// 执行SQL命令，返回影响的行数
        /// </summary>
        /// <param name="sql">SQL Command</param>
        /// <param name="paras">Parameters</param>
        /// <param name="isStoredProcedure">Is stored procedure</param>
        /// <returns>Rows affacted</returns>
        public override int Execute(string sql, IDictionary<string, dynamic> paras, bool? isStoredProcedure = false)
        {
            using (var connection = NewConnection)
            using (var command = new SqliteCommand(sql, connection))
            {
                // Add parameters
                AddParameters(command, paras);

                // Command type
                if (isStoredProcedure == null)
                    command.Prepare();
                else if (isStoredProcedure.Value)
                    command.CommandType = CommandType.StoredProcedure;

                // Open connection
                connection.Open();

                // Execute
                var result = command.ExecuteNonQuery();

                // Close
                connection.Close();

                // Return
                return result;
            }
        }

        /// <summary>
        /// Async Execute SQL Command
        /// 异步执行SQL命令
        /// </summary>
        /// <param name="sql">SQL Command</param>
        /// <param name="paras">Parameters</param>
        /// <param name="isStoredProcedure">Is stored procedure</param>
        /// <returns>Rows affacted</returns>
        public override async Task<int> ExecuteAsync(string sql, IDictionary<string, dynamic> paras, bool? isStoredProcedure = false)
        {
            using (var connection = NewConnection)
            using (var command = new SqliteCommand(sql, connection))
            {
                // Add parameters
                AddParameters(command, paras);

                // Command type
                if (isStoredProcedure == null)
                    await command.PrepareAsync();
                else if (isStoredProcedure.Value)
                    command.CommandType = CommandType.StoredProcedure;

                // Open connection
                await connection.OpenAsync();

                // Execute
                var result = await command.ExecuteNonQueryAsync();

                // Close
                await connection.CloseAsync();

                // Return
                return result;
            }
        }

        /// <summary>
        /// Execute SQL Command to return first row first column value
        /// 执行SQL命令，返回第一行第一列的值
        /// </summary>
        /// <param name="sql">SQL Command</param>
        /// <param name="paras">Parameters</param>
        /// <param name="isStoredProcedure">Is stored procedure</param>
        /// <returns>First row first column's value</returns>
        public override object ExecuteScalar(string sql, IDictionary<string, dynamic> paras, bool? isStoredProcedure = false)
        {
            using (var connection = NewConnection)
            using (var command = new SqliteCommand(sql, connection))
            {
                // Add parameters
                AddParameters(command, paras);

                // Command type
                if (isStoredProcedure == null)
                    command.Prepare();
                else if (isStoredProcedure.Value)
                    command.CommandType = CommandType.StoredProcedure;

                // Open connection
                connection.Open();

                // Execute
                var result = command.ExecuteScalar();

                // Close
                connection.Close();

                // Return
                return result;
            }
        }

        /// <summary>
        /// Async Execute SQL Command to return first row first column value
        /// 异步执行SQL命令，返回第一行第一列的值
        /// </summary>
        /// <param name="sql">SQL Command</param>
        /// <param name="paras">Parameters</param>
        /// <param name="isStoredProcedure">Is stored procedure</param>
        /// <returns>First row first column's value</returns>
        public override async Task<object> ExecuteScalarAsync(string sql, IDictionary<string, dynamic> paras, bool? isStoredProcedure = false)
        {
            using (var connection = NewConnection)
            using (var command = new SqliteCommand(sql, connection))
            {
                // Add parameters
                AddParameters(command, paras);

                // Command type
                if (isStoredProcedure == null)
                    await command.PrepareAsync();
                else if (isStoredProcedure.Value)
                    command.CommandType = CommandType.StoredProcedure;

                // Open connection
                await connection.OpenAsync();

                // Execute
                var result = await command.ExecuteScalarAsync();

                // Close
                await connection.CloseAsync();

                // Return
                return result;
            }
        }

        /// <summary>
        /// Execute SQL Command to return operation result
        /// 执行SQL命令，返回操作结果对象
        /// </summary>
        /// <param name="sql">SQL Command</param>
        /// <param name="paras">Parameters</param>
        /// <param name="isStoredProcedure">Is stored procedure</param>
        public override OperationResult ExecuteResult(string sql, IDictionary<string, dynamic> paras, bool? isStoredProcedure = false)
        {
            // Setup result
            var result = new OperationResult();

            using (var connection = NewConnection)
            using (var command = new SqliteCommand(sql, connection))
            {
                // Add parameters
                AddParameters(command, paras);

                try
                {
                    // Command type
                    if (isStoredProcedure == null)
                        command.Prepare();
                    else if (isStoredProcedure.Value)
                        command.CommandType = CommandType.StoredProcedure;

                    // Open connection
                    connection.Open();

                    // Read one line
                    using (var reader = command.ExecuteReader(CommandBehavior.SingleResult & CommandBehavior.SingleRow))
                    {
                        if (reader.Read())
                        {
                            for (var f = 0; f < reader.FieldCount; f++)
                            {
                                var name = reader.GetName(f);
                                switch (name)
                                {
                                    case "error_code":
                                        result.ErrorCode = reader.GetFieldValue<int>(f);
                                        break;
                                    case "field":
                                        result.Field = reader.GetFieldValue<string>(f);
                                        break;
                                    case "message_id":
                                        result.MessageId = reader.GetFieldValue<string>(f);
                                        break;
                                    case "mid":
                                        goto case "message_id";
                                    case "m_id":
                                        goto case "message_id";
                                    case "message":
                                        result.Message = reader.GetFieldValue<string>(f);
                                        break;
                                    default:
                                        if (!reader.IsDBNull(f))
                                        {
                                            result.Data[name] = reader.GetFieldValue<dynamic>(f);
                                        }
                                        break;
                                }
                            }
                        }

                        reader.Close();
                        reader.Dispose();
                    }

                    // Close
                    connection.Close();
                }
                catch (Exception ex)
                {
                    // Exception error
                    result.SetError(-1, "exception", ex.Message);
                }
            }

            // Return
            return result;
        }

        /// <summary>
        /// Async execute SQL Command to return operation result
        /// 异步执行SQL命令，返回操作结果对象
        /// </summary>
        /// <param name="sql">SQL Command</param>
        /// <param name="paras">Parameters</param>
        /// <param name="isStoredProcedure">Is stored procedure</param>
        public override async Task<OperationResult> ExecuteResultAsync(string sql, IDictionary<string, dynamic> paras, bool? isStoredProcedure = false)
        {
            // Setup result
            var result = new OperationResult();

            using (var connection = NewConnection)
            using (var command = new SqliteCommand(sql, connection))
            {
                // Add parameters
                AddParameters(command, paras);

                try
                {
                    // Command type
                    if (isStoredProcedure == null)
                        await command.PrepareAsync();
                    else if (isStoredProcedure.Value)
                        command.CommandType = CommandType.StoredProcedure;

                    // Open connection
                    await connection.OpenAsync();

                    // Read one line
                    using (var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleResult & CommandBehavior.SingleRow))
                    {
                        if (await reader.ReadAsync())
                        {
                            for (var f = 0; f < reader.FieldCount; f++)
                            {
                                var name = reader.GetName(f);
                                switch (name)
                                {
                                    case "error_code":
                                        result.ErrorCode = await reader.GetFieldValueAsync<int>(f);
                                        break;
                                    case "field":
                                        result.Field = await reader.GetFieldValueAsync<string>(f);
                                        break;
                                    case "message_id":
                                        result.MessageId = await reader.GetFieldValueAsync<string>(f);
                                        break;
                                    case "mid":
                                        goto case "message_id";
                                    case "m_id":
                                        goto case "message_id";
                                    case "message":
                                        result.Message = await reader.GetFieldValueAsync<string>(f);
                                        break;
                                    default:
                                        if (!await reader.IsDBNullAsync(f))
                                        {
                                            result.Data[name] = await reader.GetFieldValueAsync<dynamic>(f);
                                        }
                                        break;
                                }
                            }
                        }

                        await reader.CloseAsync();
                        await reader.DisposeAsync();
                    }

                    // Close
                    await connection.CloseAsync();
                }
                catch (Exception ex)
                {
                    // Exception error
                    result.SetError(-1, "exception", ex.Message);
                }
            }

            // Return
            return result;
        }

        /// <summary>
        /// Execute SQL Command, write to stream of the first row first column value, used to read huge text data like json/xml
        /// 执行SQL命令，读取第一行第一列的数据到流，用于读取大文本字段，比如返回的JSON/XML数据
        /// </summary>
        /// <param name="stream">Stream to write</param>
        /// <param name="sql">SQL Command</param>
        /// <param name="paras">Parameters</param>
        /// <param name="isStoredProcedure">Is stored procedure</param>
        public override void ExecuteToStream(Stream stream, string sql, IDictionary<string, dynamic> paras, bool? isStoredProcedure = false)
        {
            using (var connection = NewConnection)
            using (var command = new SqliteCommand(sql, connection))
            {
                // Add parameters
                AddParameters(command, paras);

                // Command type
                if (isStoredProcedure == null)
                    command.Prepare();
                else if (isStoredProcedure.Value)
                    command.CommandType = CommandType.StoredProcedure;

                // Open connection
                connection.Open();

                // Execute
                using (var reader = command.ExecuteReader(CommandBehavior.SingleResult & CommandBehavior.SingleRow))
                {
                    if (reader.Read())
                    {
                        // Get the TextReader
                        using (var textReader = reader.GetTextReader(0))
                        {
                            var buffer = new char[2048];
                            int read;
                            while ((read = textReader.ReadBlock(buffer, 0, buffer.Length)) > 0)
                            {
                                var bytes = Encoding.UTF8.GetBytes(buffer, 0, read);
                                stream.Write(bytes, 0, bytes.Length);
                                stream.Flush();
                            }

                            textReader.Close();
                            textReader.Dispose();
                        }
                    }

                    reader.Close();
                    reader.Dispose();
                }

                // Close
                connection.Close();
            }
        }

        /// <summary>
        /// Async ESQL Command, write to stream of the first row first column value, used to read huge text data like json/xml
        /// 异步执行SQL命令，读取第一行第一列的数据到流，用于读取大文本字段，比如返回的JSON/XML数据
        /// </summary>
        /// <param name="stream">Stream to write</param>
        /// <param name="sql">SQL Command</param>
        /// <param name="paras">Parameters</param>
        /// <param name="isStoredProcedure">Is stored procedure</param>
        public override async Task ExecuteToStreamAsync(Stream stream, string sql, IDictionary<string, dynamic> paras, bool? isStoredProcedure = false)
        {
            using (var connection = NewConnection)
            using (var command = new SqliteCommand(sql, connection))
            {
                // Add parameters
                AddParameters(command, paras);

                // Command type
                if (isStoredProcedure == null)
                    await command.PrepareAsync();
                else if (isStoredProcedure.Value)
                    command.CommandType = CommandType.StoredProcedure;

                // Open connection
                await connection.OpenAsync();

                // Execute
                using (var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleResult & CommandBehavior.SingleRow))
                {
                    if (await reader.ReadAsync())
                    {
                        // Get the TextReader
                        using (var textReader = reader.GetTextReader(0))
                        {
                            var buffer = new char[2048];
                            int read;
                            while ((read = await textReader.ReadBlockAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                var bytes = Encoding.UTF8.GetBytes(buffer, 0, read);
                                await stream.WriteAsync(bytes, 0, bytes.Length);
                                await stream.FlushAsync();
                            }

                            textReader.Close();
                            textReader.Dispose();
                        }
                    }

                    await reader.CloseAsync();
                    await reader.DisposeAsync();
                }

                // Close
                await connection.CloseAsync();
            }
        }
    }
}