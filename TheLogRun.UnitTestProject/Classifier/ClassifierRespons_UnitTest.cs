using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TheLongRun.Common.Orchestration;

namespace TheLogRun.UnitTestProject.Classifier
{
    [TestClass]
    public class ClassifierRespons_UnitTest
    {
        [TestMethod]
        public void Constructor_TestMethod()
        {
            ClassifierResponse testObj = null;
            //TODO ClassifierResponse testObj = ClassifierResponse.Create(Mocking.GetProjectionUntyped());
            Assert.IsNotNull(testObj);
        }
    }
}
