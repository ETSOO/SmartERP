using com.etsoo.Core.Utils;
using NUnit.Framework;

namespace com.etsoo.Core.UnitTests.Utils
{
    [TestFixture]
    public class StringKeyDictionaryTests
    {
        StringKeyDictionary<dynamic> dic = new StringKeyDictionary<dynamic>();

        /// <summary>
        /// Setup
        /// </summary>
        [SetUp]
        public void Setup()
        {
            dic["null"] = null;
            dic["bool"] = true;
            dic["money"] = 12.8M;
            dic["string"] = "12.8";
        }

        /// <summary>
        /// Get Null value test
        /// 获取空值测试
        /// </summary>
        [Test]
        public void GetTest_Null_Tests()
        {
            Assert.IsNull(dic.Get("null"));
            Assert.IsNull(dic.Get<bool>("null"));
            Assert.IsNull(dic.Get<bool>("money"));
        }

        /// <summary>
        /// Get value test
        /// 获取值测试
        /// </summary>
        [Test]
        public void GetTest_Value_Tests()
        {
            Assert.IsTrue(dic.Get<bool>("bool") == true);
            Assert.IsTrue(dic.Get<decimal>("money") == 12.8M);
            Assert.IsTrue(dic.Get<decimal>("string") == 12.8M);
        }
    }
}