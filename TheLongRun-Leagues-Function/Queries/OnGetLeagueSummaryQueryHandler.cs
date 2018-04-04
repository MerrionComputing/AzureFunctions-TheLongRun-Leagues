using System;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Azure.WebJobs.Host;
using TheLongRun.Common;
using TheLongRun.Common.Attributes;

namespace TheLongRunLeaguesFunction.Queries
{
    public static partial class Query
    {
        [ApplicationName("The Long Run")]
        [DomainName("Leagues")]
        [AggregateRoot("League")]
        [QueryName("Get League Summary")]
        [FunctionName("OnGetLeagueSummaryQueryHandler")]
        public static void OnGetLeagueSummaryQueryHandler([EventGridTrigger] EventGridEvent eventGridEvent,
            Binder queryLog,
            TraceWriter log
            )
        {

            const string QUERY_NAME = @"get-league-summary";

            if (null == eventGridEvent)
            {
                // This function should not proceed if there is no event data
                if (null != log)
                {
                    log.Error("Missing event grid trigger data",
                        source: "OnGetLeagueSummaryQuery");
                }
                return;
            }

            // Get the query parameters : [League Name]


            // Save the parameters for the query handler
            

            QueryLogRecord< object> qryRecord = QueryLogRecord< object>.Create(QUERY_NAME, null);

            var queryLogBlogAttribute = new BlobAttribute(QueryLogRecord.DEFAULT_CONTAINER_NAME + @"/" + QUERY_NAME  + @"/" + QueryLogRecord.MakeFilename(qryRecord),
                FileAccess.Write)
            {
                Connection = CommandLogRecord.DEFAULT_CONNECTION
            };

            using (var writer = queryLog.Bind<TextWriter>(queryLogBlogAttribute))
            {
                // persist the query to the blob

            }


            // Log that this step has completed
            if (null != log)
            {
                log.Verbose("Query passed on to handler", source: "OnGetLeagueSummaryQuery");
            }


        }
    }
}
