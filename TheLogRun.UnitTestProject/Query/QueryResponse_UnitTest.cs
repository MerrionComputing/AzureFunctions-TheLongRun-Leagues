using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TheLongRun.Common.Orchestration;

namespace TheLogRun.UnitTestProject.Query
{
    [TestClass]
    public class QueryResponse_UnitTest
    {
        [TestMethod]
        public void Constructor_TestMethod()
        {

            QueryResponse testObj = QueryResponse.Create(new DateTime(2016, 12, 14),
                null,
                null);

            Assert.IsNotNull(testObj); 

        }
    }
}
