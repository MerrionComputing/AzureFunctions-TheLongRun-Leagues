
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.IO;

using Leagues.League.queryDefinition;
using TheLongRun.Common;
using TheLongRun.Common.Attributes;
using TheLongRun.Common.Bindings;


namespace TheLongRunLeaguesFunction.Queries
{
    public static partial class Query
    {
        [ApplicationName("The Long Run")]
        [DomainName("Leagues")]
        [AggregateRoot("League")]
        [QueryName("Get League Summary")]
        [EventTopicSourceName("Get-League-Summary-Query")]
        [FunctionName("OnGetLeagueSummaryQueryHandler")]
        public static async void OnGetLeagueSummaryQueryHandler(
            [EventGridTrigger] EventGridEvent eventGridEvent,
            TraceWriter log
            )
        {

            const string QUERY_NAME = @"get-league-summary";

            #region Logging
            if (null != log)
            {
                log.Verbose("Function triggered ",
                    source: "OnGetLeagueSummaryQuery");
            }

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
            #endregion

            try
            {
                #region Logging
                if (null != log)
                {
                    log.Verbose($"Get the query parameters", source: "OnGetLeagueSummaryQuery");
                    if (null == eventGridEvent.Data )
                    {
                        log.Error($"The query parameter has no values", source: "OnGetLeagueSummaryQuery");
                        return;
                    }
                }
                #endregion
                // Get the query parameters : [League Name]

                QueryRequest<Get_League_Summary_Definition> queryRequest = eventGridEvent.Data.ToObject<QueryRequest<Get_League_Summary_Definition>>();
                if (null != queryRequest)
                {
                    QueryLogRecord<Get_League_Summary_Definition> qryRecord = QueryLogRecord<Get_League_Summary_Definition>.Create(
                        QUERY_NAME,
                        queryRequest.Parameters,
                        queryRequest.ReturnTarget,
                        queryRequest.ReturnPath);

                    if (null != qryRecord)
                    {
                        #region Logging
                        if (null != log)
                        {
                            log.Verbose ($"Query {QUERY_NAME} called for {queryRequest.Parameters.League_Name} - to return to {queryRequest.Parameters }",
                                source: "OnGetLeagueSummaryQuery");
                        }
                        #endregion

                        EventStream queryEvents = new EventStream(@"Query",
                            QUERY_NAME ,
                            qryRecord.QueryUniqueIdentifier.ToString());

                        if (null != queryEvents )
                        {
                            // Log the query creation
                            queryEvents.AppendEvent(new TheLongRun.Common.Events.Query.QueryCreated(QUERY_NAME ,
                                        qryRecord.QueryUniqueIdentifier ));

                            // set the parameters
                            queryEvents.AppendEvent(new TheLongRun.Common.Events.Query.QueryParameterValueSet
                                (nameof(queryRequest.Parameters.League_Name ), queryRequest.Parameters.League_Name ));

                            // set the return target
                            queryEvents.AppendEvent(new TheLongRun.Common.Events.Query.OutputLocationSet 
                                ( qryRecord.ReturnPath , qryRecord.ReturnTarget ));

                            // Call the next query in the command chain
                            FunctionChaining funcChain = new FunctionChaining(log);
                            var queryParams = new System.Collections.Generic.List<Tuple<string, string>>();
                            queryParams.Add (new Tuple<string, string>("queryId" , qryRecord.QueryUniqueIdentifier.ToString()) );
                            funcChain.TriggerCommandByHTTPS(@"Leagues", "GetLeagueSummaryQueryValidation", queryParams, null );  
                        }

                    }
                    else
                    {
                        #region Logging
                        if (null != log)
                        {
                            log.Error($"Query called with data that could not be converted to query log record",
                                source: "OnGetLeagueSummaryQuery");
                        }
                        #endregion
                    }

                }
                else
                {
                    #region Logging
                    if (null != log)
                    {
                        log.Error($"Query called with data that could not be converted to parameters",
                            source: "OnGetLeagueSummaryQuery");
                    }
                    #endregion
                }


            }
            catch (Exception ex)
            {
                if (null != log)
                {
                    log.Error(ex.ToString(), ex);
                }
                throw;
            }

            // Log that this step has completed
            if (null != log)
            {
                log.Verbose("Query passed on to handler", source: "OnGetLeagueSummaryQuery");
            }


        }
    }
}
