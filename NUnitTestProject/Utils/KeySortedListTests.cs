using com.etsoo.Core.Utils;
using NUnit.Framework;
using System.Collections.Generic;

namespace com.etsoo.Core.UnitTests.Utils
{
    [TestFixture]
    public class KeySortedListTests
    {
        /// <summary>
        /// Test add item
        /// </summary>
        [Test]
        public void Add_Test()
        {
            // Arrange
            var list = new KeySortedList();
            list.Add("key1", "value1");
            list.Add("key1", new List<string>() { "value2", "value3" });

            // Act and assert
            Assert.AreEqual(new List<string>() { "value1", "value2", "value3" }, list["key1"]);
        }
    }
}