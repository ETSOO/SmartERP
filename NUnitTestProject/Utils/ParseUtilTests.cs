using com.etsoo.Core.Utils;
using NUnit.Framework;
using System.Collections.Generic;
using System.Diagnostics;

namespace com.etsoo.Core.UnitTests.Utils
{
    [TestFixture]
    public class ParseUtilTests
    {
        private static IEnumerable<TestCaseData> TryParseBoolBulkTestData
        {
            get
            {
                yield return new TestCaseData(true, true);
                yield return new TestCaseData("true", true);
                yield return new TestCaseData("True", true);
                yield return new TestCaseData("TrUe", true);
                yield return new TestCaseData(1, true);
                yield return new TestCaseData("1", true);
                yield return new TestCaseData(false, false);
                yield return new TestCaseData("false", false);
                yield return new TestCaseData("False", false);
                yield return new TestCaseData(0, false);
                yield return new TestCaseData("0", false);
                yield return new TestCaseData("abc", null);
                yield return new TestCaseData(-1, null);
                yield return new TestCaseData(null, null);
            }
        }

        /// <summary>
        /// Bulk test to parse bool value
        /// </summary>
        /// <param name="input">Input data</param>
        /// <param name="expectedResult">Expected result</param>
        [Test, TestCaseSource(nameof(TryParseBoolBulkTestData))]
        public void TryParse_Bool_Bulk(object input, bool? expectedResult)
        {
            // Arange

            // Act
            var result = ParseUtil.TryParse<bool>(input);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        private static IEnumerable<TestCaseData> TryParsePerformanceBulkTestData
        {
            get
            {
                yield return new TestCaseData(true);
                yield return new TestCaseData("true");
                yield return new TestCaseData(1);
                yield return new TestCaseData("abc");
                yield return new TestCaseData(-1);
            }
        }

        /// <summary>
        /// Performance test to parse bool value, 10K less than 100ms
        /// </summary>
        /// <param name="input">Input data</param>
        [Test, TestCaseSource(nameof(TryParsePerformanceBulkTestData))]
        public void TryParse_Performance_Bulk(object input)
        {
            var sw = new Stopwatch();
            sw.Start();

            for (var i = 0; i < 10000; i++)
            {
                ParseUtil.TryParse<bool>(input);
            }

            sw.Stop();

            var ms = sw.ElapsedMilliseconds;

            Assert.IsTrue(ms < 100, $"{input}, {ms} is more than 100 ms");
        }
    }
}