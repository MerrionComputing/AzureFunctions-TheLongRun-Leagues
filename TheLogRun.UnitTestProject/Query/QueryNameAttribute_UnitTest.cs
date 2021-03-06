﻿using System;
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

        [TestMethod]
        public void Name_Modified_TestMethod()
        {
            string expected = @"Get-Cattle-Query";
            string actual = @"Get-Cattle";

            QueryNameAttribute attrTest = new QueryNameAttribute(actual );

            actual = attrTest.GetDefaultFunctionName().Name ;
            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        public void Name_NotModified_TestMethod()
        {
            string expected = @"Get-Cattle-Query";
            string actual = @"Not set";

            QueryNameAttribute attrTest = new QueryNameAttribute(expected );

            actual = attrTest.GetDefaultFunctionName().Name;
            Assert.AreEqual(expected, actual);

        }
    }
}
