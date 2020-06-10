using com.etsoo.Core.Utils;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace com.etsoo.Core.UnitTests.Utils
{
    [TestFixture]
    public class CryptographyUtilTests
    {
        /// <summary>
        /// NULL or empty string exception test
        /// </summary>
        /// <param name="value">NULL or empty string</param>
        [TestCase(null)]
        [TestCase("")]
        public void HMACSHA512_Exception_Test(string value)
        {
            Assert.Catch(() =>
            {
                CryptographyUtil.HMACSHA512(value, "***");
            });
        }

        /// <summary>
        /// Valid text test
        /// </summary>
        [Test]
        public void HMACSHA512_Valid_Test()
        {
            Assert.AreEqual("sESa+QcgEMzwmQ7o5iVpJZgnvmkZR3RQXyExRDdlW9AuNd3kxb0Pi8vaeCFLypcXTFFV8DWbBnAGSALG5xJqGg==", CryptographyUtil.HMACSHA512("1234", "]nB9]gY!)sL?"));
        }
    }
}