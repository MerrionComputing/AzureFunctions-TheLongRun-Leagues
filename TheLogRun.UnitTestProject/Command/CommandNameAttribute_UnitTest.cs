using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TheLongRun.Common.Attributes;

namespace TheLogRun.UnitTestProject.Command
{
    [TestClass]
    public class CommandNameAttribute_UnitTest
    {

        [TestMethod]
        public void Constructor_TestMethod()
        {

            CommandNameAttribute attrTest = new CommandNameAttribute(@"Sell-Cow");
            Assert.IsNotNull(attrTest);

        }

        [TestMethod]
        public void Name_Modified_TestMethod()
        {
            string expected = @"Sell-Cow-Command";
            string actual = @"Sell-Cow";

            CommandNameAttribute attrTest = new CommandNameAttribute(actual);

            actual = attrTest.GetDefaultFunctionName().Name;
            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        public void Name_NotModified_TestMethod()
        {
            string expected = @"Sell-Cow-Command";
            string actual = @"Not set";

            CommandNameAttribute attrTest = new CommandNameAttribute(expected);

            actual = attrTest.GetDefaultFunctionName().Name;
            Assert.AreEqual(expected, actual);

        }
    }
}
