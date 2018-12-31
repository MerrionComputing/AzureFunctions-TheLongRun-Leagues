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
    /// Get the unique identifiers of all the commands that fit the given parameters
    /// </summary>
    [ApplicationName("The Long Run")]
    [DomainName(Constants.Domain_Command)]
    [IdentifierGroupName("All Commands")]
    public static class CommandsIdentifierGroup
    {

        /// <summary>
        /// Get the unique identifiers of the queries that match the given input parameters
        /// </summary>
        [ApplicationName("The Long Run")]
        [DomainName(Constants.Domain_Command)]
        [IdentifierGroupName("All Commands")]
        [FunctionName("GetAllCommandsIdentifierGroup")]
        public static async Task<HttpResponseMessage> GetAllCommandsIdentifierGroupRun(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequestMessage req,
        ILogger log)
        {

            #region Logging
            if (null != log)
            {
                log.LogDebug("Function triggered HTTP in GetAllCommandsIdentifierGroup");
            }
            #endregion

            string commandName = req.RequestUri.ParseQueryString()["CommandName"];
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

            if (string.IsNullOrWhiteSpace(commandName))
            {
                // Get request body
                AllCommandsIdentifierGroup_Request data = await req.Content.ReadAsAsync<AllCommandsIdentifierGroup_Request>();
                commandName = data.CommandName;
                asOfDate = data.AsOfDate;
            }

            AllCommandsIdentifierGroup_Request request = new AllCommandsIdentifierGroup_Request()
            {
                CommandName = commandName,
                AsOfDate = asOfDate
            };

            if (string.IsNullOrWhiteSpace(commandName))
            {
                return req.CreateResponse(HttpStatusCode.BadRequest,
                    "Please pass a command name on the query string or in the request body");
            }
            else
            {
                IEnumerable<string> ret = await AllCommandsIdentifierGroupProcess(request, log);
                return req.CreateResponse(HttpStatusCode.OK, ret);
            }

        }


        /// <summary>
        /// Get the unique identifiers of the queries that match the given input parameters
        /// </summary>
        [ApplicationName("The Long Run")]
        [DomainName(Constants.Domain_Command )]
        [IdentifierGroupName("All Commands")]
        [FunctionName("GetAllCommandsIdentifierGroupActivity")]
        public static async Task<IEnumerable<string>> GetAllCommandsIdentifierGroupActivity(
                            [ActivityTrigger] DurableActivityContext context,
                            ILogger log)
        {

            AllCommandsIdentifierGroup_Request request = context.GetInput<AllCommandsIdentifierGroup_Request>();

            #region Logging
            if (null != log)
            {
                log.LogInformation($"GetAllCommandsIdentifierGroupActivity called for command: {request.CommandName } as of {request.AsOfDate} status matching {request.MatchStatus} ");
            }
            #endregion

            return await AllCommandsIdentifierGroupProcess(request, log);
        }

        /// <summary>
        /// Get all the unique identifiers of the commands that match the input request settings
        /// </summary>
        private static async Task<IEnumerable<string>> AllCommandsIdentifierGroupProcess(
            AllCommandsIdentifierGroup_Request request,
            ILogger log)
        {

            #region Logging
            if (null != log)
            {
                log.LogInformation($"Creating identifier group processor for {request.CommandName}");
            }
            #endregion

            #region Input validations
            // If no match status, default to ALL
            if (string.IsNullOrWhiteSpace(request.MatchStatus))
            {
                request.MatchStatus = @"All";
            }
            // If as of date is stupid, clear it
            if (request.AsOfDate.HasValue)
            {
                if (request.AsOfDate.Value.Year < 2000)
                {
                    request.AsOfDate = null;
                }
            }
            #endregion

            IdentifierGroup allCommands = new IdentifierGroup(Constants.Domain_Command ,
                request.CommandName ,
                request.MatchStatus);

            if (null != allCommands)
            {
                return await allCommands.GetAll(request.AsOfDate);
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
    public class AllCommandsIdentifierGroup_Request
    {

        /// <summary>
        /// The name of the command type for which we want to get all instances
        /// </summary>
        public string CommandName { get; set; }

        /// <summary>
        /// The effective date for which we want to get the commands
        /// </summary>
        public DateTime? AsOfDate { get; set; }

        /// <summary>
        /// What the status of the command we want to get is (e.g. failed, completed etc.)
        /// </summary>
        /// <remarks>
        /// If this is blank then all commands are returned
        /// </remarks>
        public string MatchStatus { get; set; }
    }
}
