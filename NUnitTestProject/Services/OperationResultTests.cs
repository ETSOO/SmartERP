using com.etsoo.Core.Services;
using com.etsoo.Core.Utils;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace com.etsoo.Core.UnitTests.Services
{
    [TestFixture]
    public class OperationResultTests
    {
        private static IEnumerable<TestCaseData> Serialize_BulkTest_Data
        {
            get
            {
                // When do equal test with string, be aware of the zero-width no-break space (byte-order mark, BOM, \U65279)
                // Serialize as XML
                yield return new TestCaseData(new OperationResult() { }, DataFormat.Xml, "﻿<root><error_code>0</error_code></root>");
                yield return new TestCaseData(new OperationResult() { ErrorCode = 1001, Field = "id", Mid = "id_error", Message = "Id error!" }, DataFormat.Xml, "﻿<root><error_code>1001</error_code><field>id</field><mid>id_error</mid><message><![CDATA[Id error!]]></message></root>");

                var xmlResult = new OperationResult() { ErrorCode = 1001, Field = "id", Mid = "id_error", Message = "Id <a> error!" };
                xmlResult.Data["bool"] = true;
                xmlResult.Data["date"] = DateTime.Parse("2020/4/5 20:30");
                xmlResult.Data["html"] = "<b>HTML</b>";
                yield return new TestCaseData(xmlResult, DataFormat.Xml, "﻿<root><error_code>1001</error_code><field>id</field><mid>id_error</mid><message><![CDATA[Id <a> error!]]></message><data><bool>true</bool><date>2020-04-05T20:30:00</date><html>&lt;b&gt;HTML&lt;/b&gt;</html></data></root>");

                var xmlErrorResult = new OperationResult() { ErrorCode = 1002, Message = "中国" };
                xmlErrorResult.Errors.Add("e1", "a");
                xmlErrorResult.Errors.Add("e1", "b");
                xmlErrorResult.Errors.Add("e2", "c");
                yield return new TestCaseData(xmlErrorResult, DataFormat.Xml, "﻿<root><error_code>1002</error_code><message><![CDATA[中国]]></message><errors><e1><item>a</item><item>b</item></e1><e2><item>c</item></e2></errors></root>");

                // Serialize as JSON
                yield return new TestCaseData(new OperationResult() { }, DataFormat.Json, "{\"error_code\":0}");
                yield return new TestCaseData(new OperationResult() { ErrorCode = 1001, Field = "id", Mid = "id_error", Message = "Id error!" }, DataFormat.Json, "{\"error_code\":1001,\"field\":\"id\",\"mid\":\"id_error\",\"message\":\"Id error!\"}");
                yield return new TestCaseData(xmlResult, DataFormat.Json, "{\"error_code\":1001,\"field\":\"id\",\"mid\":\"id_error\",\"message\":\"Id \\u003Ca\\u003E error!\",\"data\":{\"bool\":true,\"date\":\"2020-04-05T20:30:00\",\"html\":\"\\u003Cb\\u003EHTML\\u003C/b\\u003E\"}}");
                yield return new TestCaseData(xmlErrorResult, DataFormat.Json, "{\"error_code\":1002,\"message\":\"中国\",\"errors\":{\"e1\":[\"a\",\"b\"],\"e2\":[\"c\"]}}");
            }
        }

        /// <summary>
        /// Serialization test
        /// 序列化测试
        /// </summary>
        /// <param name="result">Operation result</param>
        /// <param name="format">Data format</param>
        /// <param name="expectedResult">Expected result</param>
        [Test, TestCaseSource(nameof(Serialize_BulkTest_Data))]
        public void Serialize_BulkTest(OperationResult result, DataFormat format, string expectedResult)
        {
            // Arrange
            using (var ms = new MemoryStream())
            {
                // Act
                result.Serialize(ms, format);

                // Text content
                var text = Encoding.UTF8.GetString(ms.ToArray());

                // Assert
                Assert.AreEqual(expectedResult, text);
            }
        }

        /// <summary>
        /// Async serialization test
        /// 异步序列化测试
        /// </summary>
        /// <param name="result">Operation result</param>
        /// <param name="format">Data format</param>
        /// <param name="expectedResult">Expected result</param>
        [Test, TestCaseSource(nameof(Serialize_BulkTest_Data))]
        public async Task SerializeAsync_BulkTest(OperationResult result, DataFormat format, string expectedResult)
        {
            // Arrange
            using (var ms = new MemoryStream())
            {
                // Act
                await result.SerializeAsync(ms, format);

                // Text content
                var text = Encoding.UTF8.GetString(ms.ToArray());

                // Assert
                Assert.AreEqual(expectedResult, text);
            }
        }

        /// <summary>
        /// Set error zero case test
        /// 设置0错误号测试
        /// </summary>
        [Test]
        public void SetError_Zero_Test()
        {
            // Arrange
            var result = new OperationResult();

            // Act
            Assert.Catch(() =>
            {
                result.SetError(0, "id", "id error!");
            });
        }

        /// <summary>
        /// Set error command match test
        /// 设置错误一般匹配测试
        /// </summary>
        [Test]
        public void SetError_Match_Test()
        {
            // Arrange
            var result = new OperationResult();
            var errorCode = -1;
            var field = "id";
            var message = "Id is null!";
            var mid = "id_error";

            // Act
            result.SetError(errorCode, field, message, mid);

            // Assert
            Assert.IsFalse(result.OK);
            Assert.AreEqual(errorCode, result.ErrorCode);
            Assert.AreEqual(field, result.Field);
            Assert.AreEqual(message, result.Message);
            Assert.AreEqual(mid, result.MessageId);
        }

        private static IEnumerable<TestCaseData> Update_BulkTests_Data
        {
            get
            {
                var source = new OperationResult();
                source.Data["id"] = 1001;
                source.Data["bool"] = false;

                yield return new TestCaseData(source, new OperationResult() { Field = "id", ErrorCode = 1024 }, false, null);

                var source1 = new OperationResult();
                source1.Data["id"] = 1001;
                source1.Data["bool"] = false;

                var target = new OperationResult();
                target.SetError(-1, "id", "Id is null!");
                target.Data["id"] = 1002;
                target.Data["new"] = "Hello, world!";

                yield return new TestCaseData(source1, target, true, new StringKeyDictionary<dynamic>() { { "id", 1002 }, { "bool", false } });
            }
        }

        /// <summary>
        /// Update build tests
        /// Update方法批量测试
        /// </summary>
        /// <param name="source">Source result</param>
        /// <param name="target">Target result</param>
        /// <param name="merge">Is merge</param>
        /// <param name="testData">Target data test</param>
        [Test, TestCaseSource(nameof(Update_BulkTests_Data))]
        public void Update_BulkTests(OperationResult source, OperationResult target, bool merge, StringKeyDictionary<dynamic> testData)
        {
            // Act
            source.Update(target, merge);

            // Assert
            Assert.AreEqual(target.OK, source.OK);
            Assert.AreEqual(target.ErrorCode, source.ErrorCode);
            Assert.AreEqual(target.Field, source.Field);
            Assert.AreEqual(target.MessageId, source.MessageId);
            Assert.AreEqual(target.Message, source.Message);

            if (testData != null)
            {
                foreach (var item in testData)
                {
                    Assert.AreEqual(item.Value, source.Data[item.Key]);
                }
            }
        }
    }
}
