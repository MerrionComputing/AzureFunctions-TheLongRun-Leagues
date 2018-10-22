using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TheLongRun.Common.Orchestration;

namespace TheLogRun.UnitTestProject.Identifier_Group
{
    [TestClass]
    public class IdentifierGroupMembershipSnapshotResponse_UnitTest
    {
        [TestMethod]
        public void Constructor_TestMethod()
        {
            IdentifierGroupMembershipSnapshotResponse testResp = null;
            testResp = IdentifierGroupMembershipSnapshotResponse.Create(new DateTime(1017, 12, 19),
                new List<string>() { "abc", "def" });

            Assert.IsNotNull(testResp); 

        }

        [TestMethod ]
        public void AsOfDate_RoundTrip_TestMethod()
        {
            DateTime? expected = new DateTime(2017, 12, 18);
            DateTime? actual = null;

            IdentifierGroupMembershipSnapshotResponse testResp = IdentifierGroupMembershipSnapshotResponse.Create(expected ,
    new List<string>() { "abc", "def" });

            actual = testResp.AsOfDate;

            Assert.AreEqual(expected, actual);  

        }

        [TestMethod]
        public void AsOfDate_Empty_TestMethod()
        {
            DateTime? expected = null;
            DateTime? actual = null;

            IdentifierGroupMembershipSnapshotResponse testResp = IdentifierGroupMembershipSnapshotResponse.Create(expected,
    new List<string>() { "abc", "def" });

            actual = testResp.AsOfDate;

            Assert.AreNotEqual (expected, actual);

        }

    }

}
