using System;
using System.Collections.Generic;
using System.Text;
using TheLongRun.Common;
using TheLongRun.Common.Attributes;

using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TheLongRun.Common.Bindings;
using System.Linq;

namespace TheLongRunLeaguesFunction.Identifier_Groups
{
    /// <summary>
    /// Get the unique identifiers of all the queries that fit the given parameters
    /// </summary>
    [ApplicationName("The Long Run")]
    [DomainName(Constants.Domain_Query)]
    [IdentifierGroupName("All Queries")]
    public static class QueriesIdentifierGroup
    {


        /// <summary>
        /// Get the unique identifiers of the queries that match the given input parameters
        /// </summary>
        [ApplicationName("The Long Run")]
        [DomainName(Constants.Domain_Query)]
        [IdentifierGroupName("All Queries")]
        [FunctionName("GetAllQueriesIdentifierGroup")]
        public static async Task<HttpResponseMessage> GetAllQueriesIdentifierGroupRun(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequestMessage req,
        ILogger log)
        {

            #region Logging
            if (null != log)
            {
                log.LogDebug("Function triggered HTTP in GetAllQueriesIdentifierGroup");
            }
            #endregion

            string queryName = req.RequestUri.ParseQueryString()["QueryName"];
            string asOfDateString = req.RequestUri.ParseQueryString()["AsOfDate"];
            DateTime? asOfDate = null;
            if (!string.IsNullOrWhiteSpace(asOfDateString))
            {
                DateTime dtOut;
                if (DateTime.TryParse(asOfDateString, out dtOut))
                {
                    asOfDate = dtOut;
                }
            }

            if (string.IsNullOrWhiteSpace(queryName))
            {
                // Get request body
                AllQueriesIdentifierGroup_Request data = await req.Content.ReadAsAsync<AllQueriesIdentifierGroup_Request>();
                queryName = data.QueryName;
                asOfDate = data.AsOfDate;
            }

            AllQueriesIdentifierGroup_Request request = new AllQueriesIdentifierGroup_Request()
            {
                QueryName = queryName,
                AsOfDate = asOfDate
            };

            if (string.IsNullOrWhiteSpace(queryName ))
            {
                return req.CreateResponse(HttpStatusCode.BadRequest,
                    "Please pass a query name on the query string or in the request body");
            }
            else
            {
                IEnumerable<string> ret = await AllQueriesIdentifierGroupProcess(request, log);
                return req.CreateResponse(HttpStatusCode.OK, ret);
            }

        }

        /// <summary>
        /// Get the unique identifiers of the queries that match the given input parameters
        /// </summary>
        [ApplicationName("The Long Run")]
        [DomainName(Constants.Domain_Query)]
        [IdentifierGroupName("All Queries")]
        [FunctionName("GetAllQueriesIdentifierGroupActivity")]
        public static async Task<IEnumerable<string>> GetAllQueriesIdentifierGroupActivity(
                            [ActivityTrigger] DurableActivityContext context,
                            ILogger log)
        {

            AllQueriesIdentifierGroup_Request request = context.GetInput<AllQueriesIdentifierGroup_Request>();

            #region Logging
            if (null != log)
            {
                log.LogInformation($"GetAllQueriesIdentifierGroupActivity called for query: {request.QueryName} as of {request.AsOfDate} status matching {request.MatchStatus} ");
            }
            #endregion

            return await AllQueriesIdentifierGroupProcess(request, log); 
        }

        /// <summary>
        /// Get all the unique identifiers of the queries that match the input request settings
        /// </summary>
        /// <param name="request">
        /// </param>
        private static async Task<IEnumerable<string>> AllQueriesIdentifierGroupProcess(
            AllQueriesIdentifierGroup_Request request,
            ILogger log)
        {

            #region Logging
            if (null != log)
            {
                log.LogInformation($"Creating identifier group processor for {request.QueryName }");
            }
            #endregion

            #region Input validations
            // If no match status, default to ALL
            if (string.IsNullOrWhiteSpace(request.MatchStatus  ))
            {
                request.MatchStatus = @"All";
            }
            // If as of date is stupid, clear it
            if (request.AsOfDate.HasValue )
            {
                if (request.AsOfDate.Value.Year < 2000)
                {
                    request.AsOfDate = null;
                }
            }
            #endregion

            IdentifierGroup allQueries = new IdentifierGroup(Constants.Domain_Query,
                request.QueryName ,
                request.MatchStatus );

            if (null != allQueries )
            {
                return await allQueries.GetAll(request.AsOfDate );
            }
            else
            {
                return Enumerable.Empty<string>();
            }

        }
    }

    /// <summary>
    /// Parameters passed when requesting the queries that match the given 
    /// </summary>
    public class AllQueriesIdentifierGroup_Request
    {

        /// <summary>
        /// The name of the query type for which we want to get all instances
        /// </summary>
       public  string QueryName { get; set; }

        /// <summary>
        /// The effective date for which we want to get the queries
        /// </summary>
        public DateTime? AsOfDate { get; set; }

        /// <summary>
        /// What the status of the queries we want to get is (e.g. failed, completed etc.)
        /// </summary>
        /// <remarks>
        /// If this is blank then all queries are returned
        /// </remarks>
        public string MatchStatus { get; set; }
    }
}
