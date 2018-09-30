using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TheLongRun.Common.Attributes;

namespace TheLogRun.UnitTestProject.Identifier_Group
{
    [TestClass]
    public class IdentifierGroupNameAttribute_UnitTest
    {

        [TestMethod]
        public void Constructor_TestMethod()
        {

            IdentifierGroupNameAttribute attrTest = new IdentifierGroupNameAttribute(@"Cattle-In-Shed");
            Assert.IsNotNull(attrTest);

        }

        [TestMethod]
        public void Name_Modified_TestMethod()
        {
            string expected = @"Cattle-In-Shed-IdentifierGroup";
            string actual = @"Cattle-In-Shed";

            IdentifierGroupNameAttribute attrTest = new IdentifierGroupNameAttribute(actual);

            actual = attrTest.GetDefaultFunctionName().Name;
            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        public void Name_NotModified_TestMethod()
        {
            string expected = @"Cattle-In-Shed-IdentifierGroup";
            string actual = @"Not set";

            IdentifierGroupNameAttribute attrTest = new IdentifierGroupNameAttribute(expected);

            actual = attrTest.GetDefaultFunctionName().Name;
            Assert.AreEqual(expected, actual);

        }
    }
}
