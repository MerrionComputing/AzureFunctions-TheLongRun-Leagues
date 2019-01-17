using System;
using System.Threading.Tasks;
using Leagues.League.queryDefinition;
using Microsoft.Azure.WebJobs;
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
        [FunctionName("GetLeagueSummaryCreateQueryRequestActivity")]
        public static async Task<Guid> GetLeagueSummaryCreateQueryRequestActivity(
            [ActivityTrigger] DurableActivityContext context,
            ILogger log)
        {

            #region Logging
            if (null != log)
            {
                log.LogInformation($"GetLeagueSummaryCreateQueryRequestActivity started - instance {context.InstanceId} ");
            }
            #endregion

            QueryRequest<Get_League_Summary_Definition> queryRequest = context.GetInput<QueryRequest<Get_League_Summary_Definition>>();

            if (queryRequest.QueryUniqueIdentifier.Equals(Guid.Empty))
            {
                queryRequest.QueryUniqueIdentifier = Guid.NewGuid();
            }

            // Create a new Query record to hold the event stream for this query...
            QueryLogRecord<Get_League_Summary_Definition> qryRecord = QueryLogRecord<Get_League_Summary_Definition>.Create(
                queryRequest.QueryName,
                queryRequest.GetParameters(),
                queryRequest.ReturnTarget,
                queryRequest.ReturnPath,
                queryRequest.QueryUniqueIdentifier);


            if (null != qryRecord)
            {
                EventStream queryEvents = new EventStream(Constants.Domain_Query ,
                        queryRequest.QueryName,
                        qryRecord.QueryUniqueIdentifier.ToString());


                if (null != queryEvents)
                {

                    await queryEvents.CreateIfNotExists(); 

                    // Set the context for the events to be written using
                    queryEvents.SetContext(new WriteContext("GetLeagueSummaryCreateQueryRequestActivity", context.InstanceId));

                    // Log the query creation
                    await queryEvents.AppendEvent(new TheLongRun.Common.Events.Query.QueryCreated(queryRequest.QueryName,
                                qryRecord.QueryUniqueIdentifier));
                }

                // Return the ID of the query record we created
                return qryRecord.QueryUniqueIdentifier;
            }

            // If we got here there was a problem so return an empty GUID
            return Guid.Empty;
        }


    }

}