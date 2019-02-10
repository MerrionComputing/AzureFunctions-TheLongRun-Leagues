using NUnit.Framework;
using System;
using TheLongRun.Common.Model;

namespace Tests
{
    public class Model_Domains_UnitTest
    {


        [Test]
        public void Constructor_Test()
        {
            Domains testObj = new Domains();
            Assert.IsNotNull(testObj);

        }

        [Test]
        public void Fluent_AddDomain_Test()
        {
            Domains testObj = new Domains().Add(new Domain("Test Domain")) ;
            Assert.IsNotNull(testObj["Test Domain"]);

        }

        [Test]
        public void Fluent_AddEntityType_Test()
        {
            Domains testObj = new Domains()
                .Add(new Domain("Test Domain")
                .Add(new EntityType("Test Entity", 
                   domainParentName: "Test Domain", 
                   connectionStringName: "TestEntityConnectionString") ));

            Assert.IsNotNull(testObj["Test Domain"]
                .EntityTypes["Test Entity"] );

        }

        [Test]
        public void Fluent_AddProjectionType_Test()
        {
            Domains testObj = new Domains()
                .Add(new Domain("Test Domain")
                .Add(new EntityType("Test Entity")
                .Add(new ProjectionDefinition("Test Projection"))));

            Assert.IsNotNull(testObj["Test Domain"]
                .EntityTypes["Test Entity"]
                .ProjectionDefinitions["Test Projection"]);

        }

        [Test]
        public void Fluent_AddClassifierType_Test()
        {
            Domains testObj = new Domains()
                .Add(new Domain("Test Domain")
                   .Add(new EntityType("Test Entity")
                     .Add(new ProjectionDefinition("Test Projection"))
                .Add(new ClassifierDefinition ("Test Classifier"))));

            Assert.IsNotNull(testObj["Test Domain"]
                .EntityTypes["Test Entity"]
                .ClassifierDefinitions ["Test Classifier"]);

        }


        [Test]
        public void Fluent_AddIdentifierGroup_Test()
        {
            Domains testObj = new Domains()
                .Add(new Domain("Test Domain")
                .Add(new EntityType("Test Entity")
                .Add(new IdentifierGroupDefinition ("Test Identifier Group")
                )));

            Assert.IsNotNull(testObj["Test Domain"]
                .EntityTypes["Test Entity"]
                .IdentifierGroupDefinitions ["Test Identifier Group"]);

        }

    }
}
