using Leagues.League.queryDefinition;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using TheLongRun.Common;
using TheLongRun.Common.Attributes;
using TheLongRun.Common.Bindings;
using TheLongRun.Common.Events.Query.Projections;
using TheLongRun.Common.Orchestration;

namespace TheLongRunLeaguesFunction.Queries
{

    public static partial class GetLeagueSummaryQuery
    {

        /// <summary>
        /// Log the result from a projection projections that are needed to be run to answer this query
        /// </summary>
        [ApplicationName("The Long Run")]
        [DomainName(Constants.Domain_Query )]
        [AggregateRoot("League")]
        [QueryName("Get League Summary")]
        [FunctionName("LogQueryProjectionResultActivity")]
        public static async Task<ActivityResponse> LogQueryProjectionResultActivity(
            [ActivityTrigger] DurableActivityContext context,
            ILogger log,
            CQRSAzure.EventSourcing.IWriteContext writeContext = null)
        {

            ActivityResponse resp = new ActivityResponse() { FunctionName = "LogQueryProjectionResultActivity" };

            // get the ProjectionResultsRecord
            ProjectionResultsRecord<object> data = context.GetInput<ProjectionResultsRecord<object>>();
            if (null != data)
            {
                await QueryLogRecord.LogProjectionResult(data.CorrelationIdentifier ,
                                                         data.ParentRequestName ,
                                                         data.ProjectionName ,
                                                         data.DomainName,
                                                         data.AggregateTypeName,
                                                         data.EntityUniqueIdentifier,
                                                         data.CurrentAsOfDate,
                                                         data.Result ,
                                                         data.CurrentSequenceNumber,
                                                         writeContext);

                resp.Message = $"Saved projection result to query {data.ParentRequestName} - {data.CorrelationIdentifier} ";
            }
            else
            {
                resp.Message = "Unable to get projection result from context";
                resp.FatalError = true;
            }

            return resp;

        }

        public class LogQueryProjectionResult
        {

            /// <summary>
            /// The ID of the query for which this result was returned from a projection
            /// </summary>
            public string QueryIdentifier { get; set; }

            /// <summary>
            /// The value returned from the projection
            /// </summary>
            public object ProjectionValue { get; set; }

        }
    }
}
