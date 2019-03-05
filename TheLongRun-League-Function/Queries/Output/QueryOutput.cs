using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using TheLongRun.Common.Attributes;
using System.Threading.Tasks;
using TheLongRun.Common.Orchestration;
using Microsoft.Extensions.Logging;
using TheLongRun.Common;
using System.Net.Http;
using Newtonsoft.Json;
using TheLongRun.Common.Bindings;

namespace TheLongRunLeaguesFunction.Queries.Output
{
    /// <summary>
    /// Different ways that the output from a query can be sent out from the query
    /// </summary>
    public static class QueryOutput
    {


        /// <summary>
        /// Log a specified output for the given query
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        [ApplicationName("The Long Run")]
        [FunctionName("QueryLogOutputTargetActivity")]
        public static async Task<ActivityResponse> QueryLogOutputTargetActivity(
            [ActivityTrigger] DurableActivityContext context,
            ILogger log)
        {

            ActivityResponse ret = new ActivityResponse() { FunctionName = "QueryLogOutputTargetActivity" };

            #region Logging
            if (null != log)
            {
                log.LogDebug($"Logging query output  in {ret.FunctionName} ");
            }
            #endregion

            QueryRequest<object> queryRequest = context.GetInput<QueryRequest<object>>();
            if (null != queryRequest )
            {
                EventStream queryEvents = new EventStream(Constants.Domain_Query,
                    queryRequest.QueryName,
                    queryRequest.QueryUniqueIdentifier.ToString());

                if (null != queryEvents)
                {

                    // Set the context for the events to be written using
                    queryEvents.SetContext(new WriteContext(ret.FunctionName, context.InstanceId));

                    // set the parameter(s)
                    await queryEvents.AppendEvent(new TheLongRun.Common.Events.Query.OutputLocationSet
                        (queryRequest.ReturnPath, queryRequest.ReturnTarget ));

                    if (null != log)
                    {
                        // Unable to get the request details from the orchestration
                        log.LogInformation($"{ret.FunctionName } : Set output path {queryRequest.ReturnPath} : {queryRequest.ReturnTarget} ");
                    }

                    ret.Message = $"Set output path {queryRequest.ReturnPath} : {queryRequest.ReturnTarget} ";
                }
                else
                {
                    ret.Message = $"Unable to get the event stream for {queryRequest.QueryName} : {queryRequest.QueryUniqueIdentifier }";
                    ret.FatalError = true;
                }
            }

            return ret;
        }

        /// <summary>
        /// Run the specified projection and return the results to the caller orchestration
        /// </summary>
        /// <remarks>
        /// The query identifier and results are 
        /// </remarks>
        [ApplicationName("The Long Run")]
        [FunctionName("QueryOutputToWebhookActivity")]
        public static async Task<ActivityResponse> QueryOutputToWebhookActivity(
            [ActivityTrigger] DurableActivityContext context,
            ILogger log)
        {

            ActivityResponse ret = new ActivityResponse() { FunctionName = "QueryOutputToWebhookActivity" };

            #region Logging
            if (null != log)
            {
                log.LogDebug($"Output  in {ret.FunctionName} ");
            }
            #endregion

            // Read the results 
            QueryOutputRecord<object> results = context.GetInput<QueryOutputRecord<object>>(); 
            if (null != results )
            {
                var payloadAsJSON = new StringContent(JsonConvert.SerializeObject(results.Results));
                using (var client = new HttpClient())
                {
                    var response = await client.PostAsync(results.Target, payloadAsJSON);
                    if (response.IsSuccessStatusCode  )
                    {
                        ret.Message = $"Transfer succeeded {response.StatusCode} ";
                    }
                    else
                    {
                        // We don't mark this as a fatal error as we want any successful sends to go ahead anyway
                        ret.Message = $"Webhook transfer failed {response.StatusCode} - {response.ReasonPhrase} ";
                    }
                }
            }

            return ret;

        }

    }
}
