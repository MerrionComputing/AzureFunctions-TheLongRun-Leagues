using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TheLongRun.Common.Attributes;

namespace TheLogRun.UnitTestProject.Query
{
    [TestClass]
    public class QueryNameAttribute_UnitTest
    {
        [TestMethod]
        public void Constructor_TestMethod()
        {

            QueryNameAttribute attrTest = new QueryNameAttribute(@"Get-Cattle");
            Assert.IsNotNull(attrTest);  

        }
    }
}
