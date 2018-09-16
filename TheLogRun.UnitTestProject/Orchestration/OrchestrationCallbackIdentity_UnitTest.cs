using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TheLongRun.Common.Orchestration;
using static TheLongRun.Common.Orchestration.OrchestrationCallbackIdentity;

namespace TheLogRun.UnitTestProject.Orchestration
{
    [TestClass]
    public class OrchestrationCallbackIdentity_UnitTest
    {
        [TestMethod]
        public void ClassificationFromString_Empty_TestMethod()
        {

            OrchestrationClassifications expected = OrchestrationClassifications.NotSet;
            OrchestrationClassifications actual = OrchestrationClassifications.Projection;

            actual = OrchestrationCallbackIdentity.ClassificationFromString(string.Empty);  

            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        public void ClassificationFromString_NotExpected_TestMethod()
        {

            OrchestrationClassifications expected = OrchestrationClassifications.NotSet;
            OrchestrationClassifications actual = OrchestrationClassifications.Projection;

            actual = OrchestrationCallbackIdentity.ClassificationFromString("This was not expected");

            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        public void ClassificationFromString_Null_TestMethod()
        {

            OrchestrationClassifications expected = OrchestrationClassifications.NotSet;
            OrchestrationClassifications actual = OrchestrationClassifications.Projection;

            actual = OrchestrationCallbackIdentity.ClassificationFromString(null );

            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        public void ClassificationFromString_COMMAND_TestMethod()
        {

            OrchestrationClassifications expected = OrchestrationClassifications.Command ;
            OrchestrationClassifications actual = OrchestrationClassifications.NotSet ;

            actual = OrchestrationCallbackIdentity.ClassificationFromString("COMMAND");

            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        public void ClassificationFromString_Command_TestMethod()
        {

            OrchestrationClassifications expected = OrchestrationClassifications.Command;
            OrchestrationClassifications actual = OrchestrationClassifications.NotSet;

            actual = OrchestrationCallbackIdentity.ClassificationFromString("Command");

            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        public void ClassificationFromString_command_TestMethod()
        {

            OrchestrationClassifications expected = OrchestrationClassifications.Command;
            OrchestrationClassifications actual = OrchestrationClassifications.NotSet;

            actual = OrchestrationCallbackIdentity.ClassificationFromString("command");

            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        public void ClassificationFromString_QUERY_TestMethod()
        {

            OrchestrationClassifications expected = OrchestrationClassifications.Query ;
            OrchestrationClassifications actual = OrchestrationClassifications.NotSet;

            actual = OrchestrationCallbackIdentity.ClassificationFromString("QUERY");

            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        public void ClassificationFromString_Query_TestMethod()
        {

            OrchestrationClassifications expected = OrchestrationClassifications.Query;
            OrchestrationClassifications actual = OrchestrationClassifications.NotSet;

            actual = OrchestrationCallbackIdentity.ClassificationFromString("Query");

            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        public void ClassificationFromString_query_TestMethod()
        {

            OrchestrationClassifications expected = OrchestrationClassifications.Query;
            OrchestrationClassifications actual = OrchestrationClassifications.NotSet;

            actual = OrchestrationCallbackIdentity.ClassificationFromString("query");

            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        public void ClassificationFromString_CLASSIFIER_TestMethod()
        {

            OrchestrationClassifications expected = OrchestrationClassifications.Classifier ;
            OrchestrationClassifications actual = OrchestrationClassifications.NotSet;

            actual = OrchestrationCallbackIdentity.ClassificationFromString("CLASSIFIER");

            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        public void ClassificationFromString_Classifier_TestMethod()
        {

            OrchestrationClassifications expected = OrchestrationClassifications.Classifier;
            OrchestrationClassifications actual = OrchestrationClassifications.NotSet;

            actual = OrchestrationCallbackIdentity.ClassificationFromString("Classifier");

            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        public void ClassificationFromString_classifier_TestMethod()
        {

            OrchestrationClassifications expected = OrchestrationClassifications.Classifier;
            OrchestrationClassifications actual = OrchestrationClassifications.NotSet;

            actual = OrchestrationCallbackIdentity.ClassificationFromString("classifier");

            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        public void ClassificationFromString_PROJECTION_TestMethod()
        {

            OrchestrationClassifications expected = OrchestrationClassifications.Projection;
            OrchestrationClassifications actual = OrchestrationClassifications.NotSet;

            actual = OrchestrationCallbackIdentity.ClassificationFromString("PROJECTION");

            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        public void ClassificationFromString_Projection_TestMethod()
        {

            OrchestrationClassifications expected = OrchestrationClassifications.Projection;
            OrchestrationClassifications actual = OrchestrationClassifications.NotSet;

            actual = OrchestrationCallbackIdentity.ClassificationFromString("Projection");

            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        public void ClassificationFromString_projection_TestMethod()
        {

            OrchestrationClassifications expected = OrchestrationClassifications.Projection;
            OrchestrationClassifications actual = OrchestrationClassifications.NotSet;

            actual = OrchestrationCallbackIdentity.ClassificationFromString("projection");

            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        public void ClassificationFromString_GROUP_TestMethod()
        {

            OrchestrationClassifications expected = OrchestrationClassifications.IdentifierGroup ;
            OrchestrationClassifications actual = OrchestrationClassifications.NotSet;

            actual = OrchestrationCallbackIdentity.ClassificationFromString("GROUP");

            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        public void ClassificationFromString_Group_TestMethod()
        {

            OrchestrationClassifications expected = OrchestrationClassifications.IdentifierGroup;
            OrchestrationClassifications actual = OrchestrationClassifications.NotSet;

            actual = OrchestrationCallbackIdentity.ClassificationFromString("Group");

            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        public void ClassificationFromString_group_TestMethod()
        {

            OrchestrationClassifications expected = OrchestrationClassifications.IdentifierGroup;
            OrchestrationClassifications actual = OrchestrationClassifications.NotSet;

            actual = OrchestrationCallbackIdentity.ClassificationFromString("group");

            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        public void CreateFromPath_EmptyPath_TestMethod()
        {
            string TestPath = string.Empty;

            OrchestrationCallbackIdentity testIdentity = OrchestrationCallbackIdentity.CreateFromPath(TestPath);

            Assert.IsNull(testIdentity); 

        }

        [TestMethod]
        public void CreateFromPath_NullPath_TestMethod()
        {
            string TestPath = null;

            OrchestrationCallbackIdentity testIdentity = OrchestrationCallbackIdentity.CreateFromPath(TestPath);

            Assert.IsNull(testIdentity);

        }

        [TestMethod]
        public void CreateFromPath_InvalidPath_TooLong_TestMethod()
        {
            string TestPath = "The/Command/I-have/selected/has-too/long/a-path";

            OrchestrationCallbackIdentity testIdentity = OrchestrationCallbackIdentity.CreateFromPath(TestPath);

            Assert.IsNull(testIdentity);

        }

        [TestMethod]
        public void CreateFromPath_InvalidPath_TooShort_TestMethod()
        {
            string TestPath = "The/Command";

            OrchestrationCallbackIdentity testIdentity = OrchestrationCallbackIdentity.CreateFromPath(TestPath);

            Assert.IsNull(testIdentity);

        }

        [TestMethod]
        public void CreateFromPath_ValidPath_NoDomain_TestMethod()
        {
            string TestPath = "Command/The-Command/{1234-1234-12345678}";

            OrchestrationCallbackIdentity testIdentity = OrchestrationCallbackIdentity.CreateFromPath(TestPath);

            Assert.IsNotNull (testIdentity);

        }

        [TestMethod]
        public void CreateFromPath_ValidPath_Domain_TestMethod()
        {
            string TestPath = "Domain/Query/The-Query/{1234-1234-12345678}";

            OrchestrationCallbackIdentity testIdentity = OrchestrationCallbackIdentity.CreateFromPath(TestPath);

            Assert.IsNotNull(testIdentity);

        }

        [TestMethod]
        public void CreateFromPath_ValidPath_Domain_QueryName_TestMethod()
        {
            string TestPath = "Domain/Query/The-Query/{1234-1234-12345678}";

            string expected = "The-Query";
            string actual = "Not set";

            OrchestrationCallbackIdentity testIdentity = OrchestrationCallbackIdentity.CreateFromPath(TestPath);

            actual = testIdentity.InstanceName;

            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        public void CreateFromPath_ValidPath_Domain_QueryType_TestMethod()
        {
            string TestPath = "Domain/Query/The-Query/{1234-1234-12345678}";

            OrchestrationClassifications expected = OrchestrationClassifications.Query;
            OrchestrationClassifications actual = OrchestrationClassifications.NotSet;

            OrchestrationCallbackIdentity testIdentity = OrchestrationCallbackIdentity.CreateFromPath(TestPath);

            actual = testIdentity.Classification;

            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        public void CreateFromPath_ValidPath_Domain_Identity_TestMethod()
        {
            Guid expected = new Guid("02d96365-0fbe-43e6-9517-e7c22427bde7");
            Guid actual = Guid.Empty;

            string TestPath = "Domain/Query/The-Query/02d96365-0fbe-43e6-9517-e7c22427bde7";

            OrchestrationCallbackIdentity testIdentity = OrchestrationCallbackIdentity.CreateFromPath(TestPath);

            actual = testIdentity.InstanceIdentity ;

            Assert.AreEqual(expected, actual);
        }
    }
}
