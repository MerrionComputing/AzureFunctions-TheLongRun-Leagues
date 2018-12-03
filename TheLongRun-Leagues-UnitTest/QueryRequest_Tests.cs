using Leagues.League.queryDefinition;
using NUnit.Framework;
using System;
using TheLongRun.Common;

using Newtonsoft.Json;

namespace Tests
{
    public class QueryRequest_Tests
    {


        [Test]
        public void Constructor_Test()
        {
            QueryRequest<Get_League_Summary_Definition> queryRequest = new QueryRequest<Get_League_Summary_Definition>();
            Assert.IsNotNull(queryRequest);

        }

        [Test]
        public void Serialise_To_JSon_Test()
        {
            QueryRequest<Get_League_Summary_Definition> queryRequest = new QueryRequest<Get_League_Summary_Definition>();
            queryRequest.QueryName = "Get League Definition";
            queryRequest.QueryUniqueIdentifier = Guid.NewGuid();

            string asJson = JsonConvert.SerializeObject(queryRequest);

            Assert.IsNotNull(asJson); 

        }

        [Test]
        public void Serialise_To_JSon_Parameters_Test()
        {
            QueryRequest<Get_League_Summary_Definition> queryRequest = new QueryRequest<Get_League_Summary_Definition>();
            queryRequest.QueryName = "Get League Definition";
            queryRequest.QueryUniqueIdentifier = Guid.NewGuid();

            Get_League_Summary_Definition paramData = new Get_League_Summary_Definition();
            paramData.League_Name = "Azure Cloud League";

            queryRequest.SetParameters(paramData);

            string asJson = JsonConvert.SerializeObject(queryRequest);

            QueryRequest<Get_League_Summary_Definition> deserialisedQueryRequest = null;
            if (! string.IsNullOrWhiteSpace(asJson ) )
            {
                deserialisedQueryRequest = JsonConvert.DeserializeObject<QueryRequest<Get_League_Summary_Definition>>(asJson); 
            }

            Assert.IsNotNull(deserialisedQueryRequest);

        }

        [Test]
        public void Deserialise_From_JSon_Test()
        {

            string expected = "Azure cloudy league";
            string actual = "Not set";

            QueryRequest<Get_League_Summary_Definition> queryRequest = new QueryRequest<Get_League_Summary_Definition>();
            queryRequest.QueryName = "Get League Definition";
            queryRequest.QueryUniqueIdentifier = Guid.NewGuid();

            Get_League_Summary_Definition paramData = new Get_League_Summary_Definition();
            paramData.League_Name = expected;

            queryRequest.SetParameters(paramData);

            string asJson = JsonConvert.SerializeObject(queryRequest);

            QueryRequest<Get_League_Summary_Definition> deserialisedQueryRequest = null;
            if (!string.IsNullOrWhiteSpace(asJson))
            {
                deserialisedQueryRequest = JsonConvert.DeserializeObject<QueryRequest<Get_League_Summary_Definition>>(asJson);

                actual = deserialisedQueryRequest.GetParameters().League_Name; 
            }

            Assert.AreEqual(expected, actual);

        }

    }
}