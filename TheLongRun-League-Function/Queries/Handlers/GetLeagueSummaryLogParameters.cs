using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Leagues.League.queryDefinition;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using TheLongRun.Common;
using TheLongRun.Common.Bindings;

namespace TheLongRunLeaguesFunction.Queries.Handlers
{
    public static class GetLeagueSummaryLogParameters
    {


        [FunctionName("GetLeagueSummaryLogParametersActivity")]
        public static async Task GetLeagueSummaryLogParametersActivity([ActivityTrigger] QueryRequest<Get_League_Summary_Definition> queryRequest, 
            ILogger log)
        {


            if (null != log)
            {
                // Unable to get the request details from the orchestration
                log.LogInformation($"GetLeagueSummaryLogParametersActivity : Logging parameters for query {queryRequest.QueryUniqueIdentifier} ");
            }

            EventStream queryEvents = new EventStream(@"Query",
                queryRequest.QueryName ,
                queryRequest.QueryUniqueIdentifier.ToString());

            if (null != queryEvents )
            {
                // set the parameter(s)
                await queryEvents.AppendEvent(new TheLongRun.Common.Events.Query.QueryParameterValueSet
                    (nameof(queryRequest.Parameters.League_Name), queryRequest.Parameters.League_Name));

                if (null != log)
                {
                    // Unable to get the request details from the orchestration
                    log.LogInformation($"GetLeagueSummaryLogParametersActivity : Logged parameters - League name : {queryRequest.Parameters.League_Name } ");
                }

            }

            return;
        }


    }
}