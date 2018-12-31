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
    /// Identifier group of all the leagues in the system
    /// </summary>
    [ApplicationName("The Long Run")]
    [DomainName("Leagues")]
    [IdentifierGroupName("All Leagues")]
    public static class LeaguesIdentifierGroup
    {

        /// <summary>
        /// Get the unique identifiers of the queries that match the given input parameters
        /// </summary>
        [ApplicationName("The Long Run")]
        [DomainName("Leagues")]
        [IdentifierGroupName("All Leagues")]
        [FunctionName("GetAllLeaguesIdentifierGroup")]
        public static async Task<HttpResponseMessage> GetAllLeaguesIdentifierGroupRun(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequestMessage req,
        ILogger log)
        {

            #region Logging
            if (null != log)
            {
                log.LogDebug("Function triggered HTTP in GetAllLeaguesIdentifierGroup");
            }
            #endregion

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

            if (! asOfDate.HasValue )
            {
                // Get request body
                AllCommandsIdentifierGroup_Request data = await req.Content.ReadAsAsync<AllCommandsIdentifierGroup_Request>();
                if (null != data)
                {
                    asOfDate = data.AsOfDate;
                }
            }

            LeaguesIdentifierGroup_Request request = new LeaguesIdentifierGroup_Request()
            {
                AsOfDate = asOfDate
            };


            IEnumerable<string> ret = await AllLeaguesIdentifierGroupProcess(request, log);
            return req.CreateResponse(HttpStatusCode.OK, ret);
            
        }

        /// <summary>
        /// Get the unique identifiers of the queries that match the given input parameters
        /// </summary>
        [ApplicationName("The Long Run")]
        [DomainName("Leagues")]
        [IdentifierGroupName("All Leagues")]
        [FunctionName("GetAllLeaguesIdentifierGroupActivity")]
        public static async Task<IEnumerable<string>> GetAllLeaguesIdentifierGroupActivity(
                            [ActivityTrigger] DurableActivityContext context,
                            ILogger log)
        {

            LeaguesIdentifierGroup_Request request = context.GetInput<LeaguesIdentifierGroup_Request>();

            #region Logging
            if (null != log)
            {
                log.LogInformation($"GetAllLeaguesIdentifierGroupActivity called as of {request.AsOfDate}");
            }
            #endregion

            return await AllLeaguesIdentifierGroupProcess(request, log);
        }

        /// <summary>
        /// Get all the unique identifiers of the commands that match the input request settings
        /// </summary>
        private static async Task<IEnumerable<string>> AllLeaguesIdentifierGroupProcess(
            LeaguesIdentifierGroup_Request request,
            ILogger log)
        {

            #region Logging
            if (null != log)
            {
                log.LogInformation($"Creating identifier group processor for leagues");
            }
            #endregion

            #region Input validations
            // If as of date is stupid, clear it
            if (request.AsOfDate.HasValue)
            {
                if (request.AsOfDate.Value.Year < 2000)
                {
                    request.AsOfDate = null;
                }
            }
            #endregion

            IdentifierGroup allLeagues = new IdentifierGroup("Leagues",
                "League",
                "All leagues");

            if (null != allLeagues)
            {
                return await allLeagues.GetAll(request.AsOfDate);
            }
            else
            {
                return Enumerable.Empty<string>();
            }

        }
    }


    /// <summary>
    /// Parameters passed when requesting the league identifiers 
    /// </summary>
    public class LeaguesIdentifierGroup_Request
    {

        /// <summary>
        /// The effective date for which we want to get the leagues
        /// </summary>
        public DateTime? AsOfDate { get; set; }
    }

}
