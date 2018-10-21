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
    }
}
