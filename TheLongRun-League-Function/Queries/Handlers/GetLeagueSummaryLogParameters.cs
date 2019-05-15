using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Leagues.League.queryDefinition;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using TheLongRun.Common;
using TheLongRun.Common.Attributes;
using TheLongRun.Common.Bindings;
using TheLongRun.Common.Orchestration;

namespace TheLongRunLeaguesFunction.Queries.Handlers
{
    public static partial class GetLeagueSummaryQuery
    {

        [ApplicationName("The Long Run")]
        [DomainName("Leagues")]
        [AggregateRoot("League")]
        [QueryName("Get League Summary")]
        [FunctionName("GetLeagueSummaryLogParametersActivity")]
        public static async Task<ActivityResponse> GetLeagueSummaryLogParametersActivity(
            [ActivityTrigger] DurableActivityContext context,
            ILogger log = null)
        {

           

            ActivityResponse ret = new ActivityResponse() { FunctionName = "GetLeagueSummaryLogParametersActivity" };

            try
            {
                QueryRequest<Get_League_Summary_Definition> queryRequest = context.GetInput<QueryRequest<Get_League_Summary_Definition>>();

                if (null != queryRequest)
                {
                    if (null != log)
                    {
                        // Unable to get the request details from the orchestration
                        log.LogInformation($"GetLeagueSummaryLogParametersActivity : Logging parameters for query {queryRequest.QueryUniqueIdentifier} ");
                    }

                    EventStream queryEvents = EventStream.Create(Constants.Domain_Query ,
                        queryRequest.QueryName,
                        queryRequest.QueryUniqueIdentifier.ToString());

                    if (null != queryEvents)
                    {

                        // Set the context for the events to be written using
                        queryEvents.SetContext(new WriteContext(ret.FunctionName, context.InstanceId));

                        // set the parameter(s)
                        await queryEvents.AppendEvent(new TheLongRun.Common.Events.Query.QueryParameterValueSet
                            (nameof(Get_League_Summary_Definition.League_Name), queryRequest.GetParameters().League_Name));

                        if (null != log)
                        {
                            // Unable to get the request details from the orchestration
                            log.LogInformation($"GetLeagueSummaryLogParametersActivity : Logged parameters - League name : {queryRequest.GetParameters().League_Name } ");
                        }

                        ret.Message = $"Logged parameters - League name : {queryRequest.GetParameters().League_Name } ";
                    }
                    else
                    {
                        ret.Message = $"Unable to get the event stream for {queryRequest.QueryName} : {queryRequest.QueryUniqueIdentifier }";
                        ret.FatalError = true;
                    }
                }
                else
                {
                    ret.Message = $"Unable to get the request details from {context.ToString()} ";
                    ret.FatalError = true;
                }
            }
            catch (Exception ex)
            {
                if (null != log)
                {
                    // Unable to get the request details from the orchestration
                    log.LogError ($"GetLeagueSummaryLogParametersActivity : error {ex.Message} ");
                }
                ret.Message = ex.Message;
                ret.FatalError = true;
            }
            return ret;
        }


    }
}