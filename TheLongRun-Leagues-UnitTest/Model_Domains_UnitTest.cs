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

        [Test]
        public void Fluent_BiggerModel_Test()
        {

            Domains testDomains =  new Domains()
                .Add(new Domain("Leagues")
                  .Add(new EntityType("League", domainParentName: "Leagues", connectionStringName: "LeagueStorageConnectionString")
                    .Add(new ProjectionDefinition("Command Status", "GetCommandStatusInformationProjection"))
                    .Add(new IdentifierGroupDefinition("All Leagues", "GetAllLeaguesIdentifierGroup")))
                  .Add(new CommandDefinition("Create League", "OnCreateLeagueCommand"))
                  .Add(new CommandDefinition("Set Email Address", "OnSetLeagueEmailAddressCommandHandler"))
                  .Add(new QueryDefinition("Get League Summary", "OnGetLeagueSummaryQueryHandler"))
                );


            Assert.IsNotNull(testDomains["Leagues"]
                .EntityTypes["League"]
                .IdentifierGroupDefinitions["All Leagues"]);
        }



    }
}
