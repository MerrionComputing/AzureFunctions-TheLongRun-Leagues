using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using CQRSAzure.EventSourcing;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using TheLongRun.Common.Bindings;

namespace TheLongRun.Common
{
    public class QueryLogRecord
    {


        /// <summary>
        /// The folder that new queries are logged to
        /// </summary>
        public const string DEFAULT_CONTAINER_NAME = @"query-log";

        /// <summary>
        /// The connection string name to use to write to it
        /// </summary>
        public const string DEFAULT_CONNECTION = @"QueryStorageConnectionString";

        /// <summary>
        /// The unique identifier of this instance of the query
        /// </summary>
        public Guid QueryUniqueIdentifier { get; internal set; }

        /// <summary>
        /// The name of the query that was executed
        /// </summary>
        public string QueryName { get; internal set; }


        /// <summary>
        /// The status of the query when last executed
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Who triggered this query
        /// </summary>
        public string Who { get; set; }

        /// <summary>
        /// When was this command query
        /// </summary>
        public Nullable<DateTime> When { get; set; }

        /// <summary>
        /// Hash of the query name and parameters used to see if two queries are for the same thing
        /// </summary>
        /// <remarks>
        /// This can be used for results cacheing
        /// </remarks>
        public string Hash { get; }


        public static string MakeFilename(QueryLogRecord queryInstance)
        {
            return queryInstance.QueryName.ToLowerInvariant().Trim() + @"-" + queryInstance.QueryUniqueIdentifier.ToString(@"N") + @".qry";
        }

        /// <summary>
        /// Log a particular parameter validation error to the given query record
        /// </summary>
        /// <param name="queryGuid">
        /// Unique identifier of the query that had the error
        /// </param>
        /// <param name="queryName">
        /// Name of the query that had the error
        /// </param>
        /// <param name="fatalError">
        /// Is the error considered fatal
        /// </param>
        /// <param name="errorMessage">
        /// Message for the validation error
        /// param>
        public static async Task LogQueryValidationError(Guid queryGuid, 
            string queryName, 
            bool fatalError, 
            string errorMessage,
            IWriteContext writeContext = null)
        {

            EventStream qryEvents = new EventStream(Constants.Domain_Query,
            queryName ,
            queryGuid.ToString());
            if (null != qryEvents )
            {
                if (null != writeContext )
                {
                    qryEvents.SetContext(writeContext);
                }
                await qryEvents.AppendEvent(new TheLongRun.Common.Events.Query.QueryParameterValidationErrorOccured (queryName , 
                    fatalError, 
                    errorMessage ));
            }
        }

        public static async Task RequestProjection(Guid queryGuid,
            string queryName,
            string projectionName,
            string domainName,
            string aggregateTypeName,
            string aggregateInstanceKey,
            Nullable<DateTime > asOfDate,
            IWriteContext writeContext = null)
        {
            EventStream qryEvents = new EventStream(Constants.Domain_Query,
            queryName,
            queryGuid.ToString());
            if (null != qryEvents)
            {
                if (null != writeContext)
                {
                    qryEvents.SetContext(writeContext);
                }
                await qryEvents.AppendEvent(new TheLongRun.Common.Events.Query.ProjectionRequested(domainName,
                    aggregateTypeName ,
                    WebUtility.UrlDecode(aggregateInstanceKey),
                    WebUtility.UrlDecode(projectionName),
                    asOfDate));
            }
        }

        public static async Task LogProjectionStarted(
            Guid queryGuid,
            string queryName,
            string projectionTypeName,
            string domainName,
            string aggregateTypeName,
            string aggregateInstanceKey,
            Nullable<DateTime> asOfDate,
            string projectionRunneridentifier,
            IWriteContext writeContext = null)
        {
            EventStream qryEvents = new EventStream(Constants.Domain_Query,
            queryName,
            queryGuid.ToString());

            if (null != qryEvents)
            {
                if (null != writeContext)
                {
                    qryEvents.SetContext(writeContext);
                }

                await qryEvents.AppendEvent(new TheLongRun.Common.Events.Query.ProjectionRunStarted(
                    domainName,
                    aggregateTypeName,
                    WebUtility.UrlDecode(aggregateInstanceKey),
                    WebUtility.UrlDecode(projectionTypeName),
                    asOfDate.GetValueOrDefault(DateTime.UtcNow),
                    projectionRunneridentifier
                    ));
            }
        }

        public static async Task LogProjectionResult(Guid queryGuid, 
            string queryName, 
            string projectionTypeName, 
            string domainName, 
            string aggregateTypeName, 
            string aggregateInstanceKey, 
            Nullable<DateTime> asOfDate,
            object projectionValues,
            int sequenceNumber,
            IWriteContext writeContext = null)
        {
            EventStream qryEvents = new EventStream(Constants.Domain_Query,
            queryName,
            queryGuid.ToString());
            if (null != qryEvents)
            {
                if (null != writeContext)
                {
                    qryEvents.SetContext(writeContext);
                }

                await qryEvents.AppendEvent(new TheLongRun.Common.Events.Query.ProjectionValueReturned (domainName,
                    aggregateTypeName,
                    WebUtility.UrlDecode(aggregateInstanceKey),
                    WebUtility.UrlDecode(projectionTypeName),
                    asOfDate.GetValueOrDefault(DateTime.UtcNow) ,
                    projectionValues,
                    sequenceNumber ));
            }
        }

        /// <summary>
        /// Send the output to the specified target
        /// </summary>
        /// <param name="location"></param>
        /// <param name="queryReturnTarget"></param>
        /// <param name="value"></param>
        public static void SendOutput(string location, 
            QueryResponseTarget.ReturnTargetType queryReturnTarget, 
            object value)
        {
            if (null != value )
            {
                string valueAsJson = Newtonsoft.Json.JsonConvert.SerializeObject(value);
                switch (queryReturnTarget )
                {
                    case QueryResponseTarget.ReturnTargetType.AzureBlobStorage:
                        {
                            SendOutputToBlob(location, valueAsJson);
                            break;
                        }
                    case QueryResponseTarget.ReturnTargetType.CustomEventGridTopic:
                        {
                            SendOutputToCustomTopic(location, valueAsJson);
                            break;
                        }
                    case QueryResponseTarget.ReturnTargetType.WebHook:
                        {
                            SendOutputToWebhook(location, valueAsJson);
                            break;
                        }
                    case QueryResponseTarget.ReturnTargetType.DurableFunctionOrchestration:
                        {
                            // TODO: Work out how to send output to trigger a durable function to awaken
                            break;
                        }
                    case QueryResponseTarget.ReturnTargetType.ServiceBus:
                        {
                            // TODO : Send the output to a service bus endpoint
                            break;
                        }
                    case QueryResponseTarget.ReturnTargetType.SignalR:
                        {
                            // TODO: Send the output to a SignalR endpoint
                            break;
                        }
                    case QueryResponseTarget.ReturnTargetType.NotSet:
                        {
                            // Nothing to do here
                            break;
                        }
                }
            }
        }

        /// <summary>
        /// Send the output as JSON to the supplied webhook
        /// </summary>
        /// <param name="location">
        /// The URI of the webhook to send the value to
        /// </param>
        /// <param name="valueAsJson">
        /// The query result as JSON
        /// </param>
        private static void SendOutputToWebhook(string location, 
            string valueAsJson,
            ILogger log = null)
        {

            #region Logging
            if (null != log)
            {
                log.LogDebug($"Sending query results to webhook {location} ");
            }
            #endregion

            FunctionChaining outputWebhook = new FunctionChaining(log);
            outputWebhook.SendQueryResultsByWebhook(location, valueAsJson); 
        }

        /// <summary>
        /// Send the value as JSON to the event grid custom topic 
        /// </summary>
        /// <param name="location">
        /// The identifier of the event grid location to send the value to
        /// </param>
        /// <param name="valueAsJson">
        /// The query result encoded as JSON that gets put in the "data" element.
        /// </param>
        /// <remarks>
        /// See https://docs.microsoft.com/en-us/azure/event-grid/post-to-custom-topic
        /// </remarks>
        private static void SendOutputToCustomTopic(string location, 
            string valueAsJson)
        {
            // TODO: Write to the Event Grid topic specified
        }

        /// <summary>
        /// Write the value as JSON to the named Azure blob
        /// </summary>
        /// <param name="location">
        /// The target to write the JSON value to
        /// </param>
        /// <param name="valueAsJson">
        /// The query results encoded as JSON
        /// </param>
        private static void SendOutputToBlob(string location, 
            string valueAsJson)
        {
            // TODO: Write to the BLOB specified
        }
    }

    public class QueryLogRecord<TQueryParameters>
        : QueryLogRecord 
    {




        /// <summary>
        /// The parameters of the query instance - these will be saved as JSON
        /// </summary>
        public TQueryParameters Parameters { get; set; }




        /// <summary>
        /// Create a new command log record
        /// </summary>
        /// <param name="commandName">
        /// The name of the command we are going to log
        /// </param>
        public static QueryLogRecord<TQueryParameters> Create(string queryName,
            TQueryParameters parameters,
            QueryResponseTarget[] responseTargets,
            Guid queryId )
        {
            QueryLogRecord< TQueryParameters> ret = new QueryLogRecord<TQueryParameters>();
            if (queryId.Equals(Guid.Empty ))
            {
                queryId = Guid.NewGuid();
            }
            ret.QueryUniqueIdentifier = queryId ;
            ret.QueryName  = queryName;
            ret.When = DateTime.UtcNow;
            if (null != parameters )
            {
                ret.Parameters = parameters;
            }
            return ret;

        }


    }
}
