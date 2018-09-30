using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TheLongRun.Common.Attributes;

namespace TheLogRun.UnitTestProject.Projection
{
    [TestClass]
    public class ProjectionNameAttribute_UnitTest
    {

        [TestMethod]
        public void Constructor_TestMethod()
        {

            ProjectionNameAttribute attrTest = new ProjectionNameAttribute(@"Account-Balance");
            Assert.IsNotNull(attrTest);

        }

        [TestMethod]
        public void Name_Modified_TestMethod()
        {
            string expected = @"Account-Balance-Projection";
            string actual = @"Account-Balance";

            ProjectionNameAttribute attrTest = new ProjectionNameAttribute(actual);

            actual = attrTest.GetDefaultFunctionName().Name;
            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        public void Name_NotModified_TestMethod()
        {
            string expected = @"Account-Balance-Projection";
            string actual = @"Not set";

            ProjectionNameAttribute attrTest = new ProjectionNameAttribute(expected);

            actual = attrTest.GetDefaultFunctionName().Name;
            Assert.AreEqual(expected, actual);

        }
    }
}
