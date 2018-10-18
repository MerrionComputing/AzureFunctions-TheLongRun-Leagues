using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TheLongRun.Common.Events.Projection;

namespace TheLogRun.UnitTestProject.Projection
{
    [TestClass]
    public class SnapshotTaken_UnitTest
    {
        [TestMethod]
        public void Constructor_TestMethod()
        {

            SnapshotTaken testEvt = new SnapshotTaken("123", new DateTime(2018, 08, 12), 12,
                "Unit testing", @"C:\Temp");

            Assert.IsNotNull(testEvt); 

        }
    }
}
