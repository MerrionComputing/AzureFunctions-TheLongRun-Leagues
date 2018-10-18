using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TheLongRun.Common.Attributes;
using Microsoft.Azure.WebJobs;

namespace TheLogRun.UnitTestProject.Classifier
{
    [TestClass]
    public class ClassifierNameAttribute_UnitTest
    {

        [TestMethod]
        public void Constructor_TestMethod()
        {

            ClassifierNameAttribute attrTest = new ClassifierNameAttribute(@"Is-In-Barn");
            Assert.IsNotNull(attrTest);

        }

        [TestMethod]
        public void Name_Modified_TestMethod()
        {
            string expected = @"Is-In-Barn-Classifier";
            string actual = @"Is-In-Barn";

            ClassifierNameAttribute attrTest = new ClassifierNameAttribute(actual);

            actual = attrTest.GetDefaultFunctionName().Name;
            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        public void Name_NotModified_TestMethod()
        {
            string expected = @"Is-In-Barn-Classifier";
            string actual = @"Not set";

            ClassifierNameAttribute attrTest = new ClassifierNameAttribute(expected);

            actual = attrTest.GetDefaultFunctionName().Name;
            Assert.AreEqual(expected, actual);

        }

    }
}
