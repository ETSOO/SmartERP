using com.etsoo.Core.Database;
using Microsoft.Data.SqlClient;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;

namespace com.etsoo.Core.UnitTests.Database
{
    [TestFixture]
    public class SqlServerDatabaseTests
    {
        SqlServerDatabase db;

        /// <summary>
        /// Setup
        /// 初始化
        /// </summary>
        [SetUp]
        public void Setup()
        {
            // Arrange
            // Create the dabase
            db = new SqlServerDatabase("Initial Catalog=ftp_server;Server=(local);User ID=ftp;Password=ftp;Enlist=false");
        }

        /// <summary>
        /// Constructor test
        /// 构造函数测试
        /// </summary>
        [Test]
        public void SqliteDatabase_Constructor_Test()
        {
            // Act & Asset
            Assert.DoesNotThrow(() =>
            {
                using (var connection = db.NewConnection)
                {
                    connection.Open();
                    connection.Close();
                }
            });
        }

        /// <summary>
        /// Add data parameter test
        /// 测试添加数组参数
        /// </summary>
        [Test]
        public void AddDataParameter_Test()
        {
            using (var connection = db.NewConnection)
            {
                using (var command = new SqlCommand("SELECT @ids AS result", connection))
                {
                    // Parameters
                    var paras = new Dictionary<string, dynamic>();
                    db.AddDataParameter(new[] { 1, 2, 3 }, paras, "ids", false);

                    // Add parameters
                    db.AddParameters(command, paras);

                    // Act
                    connection.Open();
                    var result = command.ExecuteScalar();
                    connection.Close();

                    // Assert
                    Assert.AreEqual("1,2,3", result);
                }
            }
        }

        private static IEnumerable<TestCaseData> AddParameters_BulkTest_Data
        {
            get
            {
                // Null dictionary
                yield return new TestCaseData(null, 0);

                // Blank dictionary
                yield return new TestCaseData(new Dictionary<string, dynamic>() { }, 0);

                // Dictionary with null value item
                yield return new TestCaseData(new Dictionary<string, dynamic>() { { "test", null } }, 0);

                // Mixed dictionary
                yield return new TestCaseData(new Dictionary<string, dynamic>() { { "test1", true }, { "test2", null } }, 1);

                // More items
                yield return new TestCaseData(new Dictionary<string, dynamic>() { { "test1", true }, { "test2", null }, { "test3", "Hello, world" }, { "test4", DateTime.Now }, { "test5", new SqlParameter("test5", SqlDbType.NVarChar, 128) {  Value = "Hello, world! Hello, world! Hello, world!" } } }, 4);
            }
        }

        /// <summary>
        /// Test adding parameters to command
        /// 测试给命令添加参数
        /// </summary>
        [Test, TestCaseSource(nameof(AddParameters_BulkTest_Data))]
        public void AddParameters_BulkTest(Dictionary<string, dynamic> paras, int expectedItems)
        {
            // Create a command
            using (var connection = db.NewConnection)
            {
                using (var command = connection.CreateCommand())
                {
                    // Act
                    db.AddParameters(command, paras);

                    // Assert
                    Assert.AreEqual(expectedItems, command.Parameters.Count);
                }
            }
        }

        /// <summary>
        /// Test Creating EF Database Context
        /// 测试创建EF数据库上下文
        /// </summary>
        [Test]
        public void CreateContext_Test()
        {
            // Arrange
            // Create EF database context
            var context = db.CreateContext<TestUserModule>();

            // Act
            // Try to add two records
            context.Entities.AddRange(new[] { new TestUserModule { Id = 1001, Name = "Admin 1" }, new TestUserModule { Id = 1002, Name = "Admin 2" } });
            var result = context.SaveChanges();

            // Assert
            Assert.IsTrue(result == 2);
        }

        /// <summary>
        /// Async test Creating EF Database Context
        /// 异步测试创建EF数据库上下文
        /// </summary>
        [Test]
        public async Task CreateContextAsync_Test()
        {
            // Create EF database context
            var context = db.CreateContext<TestUserModule>();

            // Act
            // Try to add two records
            context.Entities.AddRange(new[] { new TestUserModule { Id = 1001, Name = "Admin 1" }, new TestUserModule { Id = 1002, Name = "Admin 2" } });
            var result = await context.SaveChangesAsync();

            // Assert
            Assert.IsTrue(result == 2);
        }

        /// <summary>
        /// Test Executing SQL Command
        /// 测试执行SQL 命令
        /// </summary>
        [Test]
        public void Execute_Test()
        {
            // Arrange
            // Parameters
            var paras = new Dictionary<string, dynamic>() { { "user1", 1001 }, { "name1", "Admin 1" }, { "user2", 1002 }, { "name2", "Admin 2" } };

            // Act
            // Insert rows
            var sql = "INSERT INTO e_user (id, name) VALUES(@user1, @name1), (@user2, @name2)";

            // Result
            var result = db.Execute(sql, paras, false);

            // Assert
            Assert.IsTrue(result == 2);
        }

        /// <summary>
        /// Async Test Executing SQL Command
        /// 异步测试执行SQL 命令
        /// </summary>
        [Test]
        public async Task ExecuteAsync_Test()
        {
            // Arrange
            // Parameters
            var paras = new Dictionary<string, dynamic>() { { "user1", 1001 }, { "name1", "Admin 1" }, { "user2", 1002 }, { "name2", "Admin 2" } };

            // Act
            // Insert rows
            var sql = "INSERT INTO e_user (id, name) VALUES(@user1, @name1), (@user2, @name2)";

            // Result
            var result = await db.ExecuteAsync(sql, paras, false);

            // Assert
            Assert.IsTrue(result == 2);
        }

        /// <summary>
        /// Async Test Executing SQL stored procedure command
        /// 异步测试执行SQL存储过程命令
        /// </summary>
        [Test]
        public async Task ExecuteAsync_StoredProcedure_Test()
        {
            // Arrange
            // Parameters
            var paras = new Dictionary<string, dynamic>() { { "id", new SqlParameter("id", SqlDbType.Int) { Value = 1003 } }, { "name", "Admin 3" } };

            // Act
            // Result
            var result = await db.ExecuteScalarAsync("ep_add_user", paras, true);

            // Assert
            Assert.AreEqual(1003, result);
        }

        /// <summary>
        /// Test Executing SQL Command to return first row first column value
        /// 测试执行SQL命令，返回第一行第一列的值
        /// </summary>
        [Test]
        public void ExecuteScalar_Test()
        {
            // Act
            var result = db.ExecuteScalar("SELECT name FROM e_user WHERE id = 1001");

            // Assert
            Assert.AreEqual("Admin 1", result);
        }

        /// <summary>
        /// Async test Executing SQL Command to return first row first column value
        /// 测试异步执行SQL命令，返回第一行第一列的值
        /// </summary>
        [Test]
        public async Task ExecuteScalarAsync_Test()
        {
            // Act
            var result = await db.ExecuteScalarAsync("SELECT name FROM e_user WHERE id = 1001");

            // Assert
            Assert.AreEqual("Admin 1", result);
        }

        /// <summary>
        /// Test Execute SQL Command to return operation result
        /// 测试执行SQL命令返回操作结果
        /// </summary>
        [Test]
        public void ExecuteResult_Test()
        {
            // Arrange
            var errorCode = 1001;
            var field = "id";
            var message = "Id is null!";
            var mid = "id_error";
            var data = true;

            // Act
            var result = db.ExecuteResult("SELECT @error_code AS error_code, @field AS field, @message AS message, @mid AS m_id, @data AS data", new Dictionary<string, dynamic>() { { "error_code", errorCode }, { "field", field }, { "message", message }, { "mid", mid }, { "data", data } }, false);

            // Assert
            Assert.AreEqual(errorCode, result.ErrorCode);
            Assert.AreEqual(field, result.Field);
            Assert.AreEqual(message, result.Message);
            Assert.AreEqual(mid, result.MessageId);
            Assert.AreEqual(errorCode, result.ErrorCode);
            Assert.AreEqual(result.Data.Get<bool>("data"), data);
        }

        /// <summary>
        /// Async test Execute SQL Command to return operation result
        /// 异步测试执行SQL命令返回操作结果
        /// </summary>
        [Test]
        public async Task ExecuteResultAsync_Test()
        {
            // Arrange
            var errorCode = 1001;
            var field = "id";
            var message = "Id is null!";
            var mid = "id_error";
            var data = true;

            // Act
            var result = await db.ExecuteResultAsync("SELECT @error_code AS error_code, @field AS field, @message AS message, @mid AS m_id, @data AS data", new Dictionary<string, dynamic>() { { "error_code", errorCode }, { "field", field }, { "message", message }, { "mid", mid }, { "data", data } }, false);

            // Assert
            Assert.AreEqual(errorCode, result.ErrorCode);
            Assert.AreEqual(field, result.Field);
            Assert.AreEqual(message, result.Message);
            Assert.AreEqual(mid, result.MessageId);
            Assert.AreEqual(errorCode, result.ErrorCode);
            Assert.AreEqual(result.Data.Get<bool>("data"), data);
        }

        /// <summary>
        /// Test executing SQL Command to return TextReader of first row first column value, used to read huge text data like json/xml
        /// 测试执行SQL命令，返回第一行第一列的TextReader值，用于读取大文本字段，比如返回的JSON/XML数据
        /// </summary>
        [Test]
        public void ExecuteToStream_Test()
        {
            // Arrange
            using (var stream = new MemoryStream())
            {
                // Act
                // {"name":"Admin 1"}
                db.ExecuteToStream(stream, "SELECT TOP 1 name FROM e_user WHERE id = 1001 FOR JSON PATH, WITHOUT_ARRAY_WRAPPER");

                // Assert
                Assert.IsTrue(stream.Length == "{\"name\":\"Admin 1\"}".Length);
            }
        }

        /// <summary>
        /// Async test executing SQL Command to return TextReader of first row first column value, used to read huge text data like json/xml
        /// 异步测试执行SQL命令，返回第一行第一列的TextReader值，用于读取大文本字段，比如返回的JSON/XML数据
        /// </summary>
        [Test]
        public async Task ExecuteToStreamAsync_Test()
        {
            // Arrange
            using (var stream = new MemoryStream())
            {
                // Act
                // {"name":"Admin 1"}
                await db.ExecuteToStreamAsync(stream, "SELECT TOP 1 name FROM e_user WHERE id = 1001 FOR JSON PATH, WITHOUT_ARRAY_WRAPPER");

                // Assert
                Assert.IsTrue(stream.Length == "{\"name\":\"Admin 1\"}".Length);
            }
        }
    }
}