using NUnit.Framework;
using System;
using TheLongRun.Common;
using TheLongRun.Common.Bindings;
using Leagues.League.projection;
using TheLongRun.Common.Attributes;
using System.Collections.Generic;
using CQRSAzure.EventSourcing;
using System.Linq;

namespace Tests
{
    public class Projection_UnitTest
    {
        [Test]
        public void Projection_Attribute_Name_Test()
        {

            string expected = "League Summary Information";
            string actual = "Not set";

            actual = TheLongRun.Common.Attributes.ProjectionNameAttribute.GetProjectionName(typeof(League_Summary_Information ));

            Assert.AreEqual(expected, actual);

        }

        [Test]
        public void CreateProjectionInstance_Test()
        {

            var testProjection = Projection.CreateProjectionInstance("League Summary Information");
            Assert.IsNotNull(testProjection);

        }

        [Test]
        public void MakeValidPropertyName_Test()
        {

            string expected = "My_Valid_Property";
            string actual = "Not set";



            actual = Projection.MakeValidPropertyName("My!Valid Property");
            Assert.AreEqual(expected, actual);

        }

        [Test]
        public void GetProjectionResults_Test()
        {
            int expected = 3;
            int actual = 0;


            List<ProjectionSnapshotProperty> properties = new List<ProjectionSnapshotProperty>();
            properties.Add(ProjectionSnapshotProperty.Create<string>("Title", "Mr.", 0));
            properties.Add(ProjectionSnapshotProperty.Create<string>("Title", "Ms.", 1));
            properties.Add(ProjectionSnapshotProperty.Create<string>("Title", "Mr.", 2));
            properties.Add(ProjectionSnapshotProperty.Create<string>("Forename", "Duncan", 0));
            properties.Add(ProjectionSnapshotProperty.Create<string>("Surname", "Jones", 0));
            properties.Add(ProjectionSnapshotProperty.Create<double>("Salary", 34290D, 2));

            IEnumerable<object> asObjects = Projection.GetProjectionResults(properties);
            actual = asObjects.Count();

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void GetProjectionResults_WithSpaces_Test()
        {
            string expected = "The Long Run Leagues";
            string actual = "Not Set";

            List<ProjectionSnapshotProperty> properties = new List<ProjectionSnapshotProperty>();
            properties.Add(ProjectionSnapshotProperty.Create<string>("Title", "Mr.", 0));
            properties.Add(ProjectionSnapshotProperty.Create<string>("Title", "Ms.", 1));
            properties.Add(ProjectionSnapshotProperty.Create<string>("Title", "Mr.", 2));
            properties.Add(ProjectionSnapshotProperty.Create<string>("Forename", "Duncan", 0));
            properties.Add(ProjectionSnapshotProperty.Create<string>("Surname", "Jones", 0));
            properties.Add(ProjectionSnapshotProperty.Create<string>("Domain", expected, 0));

            IEnumerable<object> asObjects = Projection.GetProjectionResults(properties);
            dynamic  first = asObjects.FirstOrDefault();

            actual = first.Domain;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ProjectionRequest_RouwndTrip_Test()
        {

            string expected = "The long run leagues";
            string actual = "Not set";

            ProjectionRequest testObj = new ProjectionRequest() {
            ParentRequestName = "Get League Details",
            DomainName= "Leagues",
            AggregateTypeName= "League",
            EntityUniqueIdentifier = "The long run leagues",
            AsOfDate=null,
            ProjectionName = "League Detail Information",
            CorrelationIdentifier= Guid.NewGuid()};

            testObj.UrlEncode();
            testObj.UrlDecode(); 

            actual = testObj.EntityUniqueIdentifier;


            Assert.AreEqual(expected, actual);
        }

    }
}
