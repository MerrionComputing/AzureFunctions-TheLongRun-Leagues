using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TheLongRun.Common;
using TheLongRun.Common.Attributes;
using TheLongRun.Common.Bindings;
using TheLongRun.Common.Events.Command.Projections;

using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System.Net;

namespace TheLongRunLeaguesFunction.Projections
{

    /// <summary>
    /// Get the current status of a given command by running the status information projection over it
    /// </summary>
    [ApplicationName("The Long Run")]
    [DomainName("Commands")]
    [AggregateRoot("Command")]
    [ProjectionName("Command Summary")]
    public static class CommandStatusInformationProjection
    {

        /// <summary>
        /// Get the current status of a given command by running the status information projection over it and return it to
        /// a HTTP call
        /// </summary>
        /// <param name="req">
        /// The request containing the parameters
        /// </param>
        /// <param name="log"></param>
        [ApplicationName("The Long Run")]
        [DomainName("Commands")]
        [AggregateRoot("Command")]
        [ProjectionName("Command Summary")]
        [FunctionName("GetCommandStatusInformationProjection")]
        public static async Task<HttpResponseMessage> GetCommandStatusInformationProjectionRun(
                [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequestMessage req,
                ILogger log)
        {
            #region Logging
            if (null != log)
            {
                log.LogDebug("Function triggered HTTP in GetCommandStatusInformationProjection");
            }
            #endregion

            // get the command id and command name
            string commandId;
            string commandName;

            commandId = req.GetQueryNameValuePairsExt()[@"CommandId"];
            commandName = req.GetQueryNameValuePairsExt()[@"CommandName"];

            if (commandId == null)
            {
                // Get request body
                dynamic data = await req.Content.ReadAsAsync<object>();
                commandId = data?.CommandId;
                commandName = data?.CommandName;
            }

            // Run the projection....ProcessCommandStatusInformationProjection
            Command_Summary_Projection_Return ret = await ProcessCommandStatusInformationProjection(commandId,
                commandName,
                log);


            return string.IsNullOrWhiteSpace(commandId)
        ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a command Id and command name on the query string or in the request body")
        : req.CreateResponse(HttpStatusCode.OK, ret);
        }


        [ApplicationName("The Long Run")]
        [DomainName("Commands")]
        [AggregateRoot("Command")]
        [ProjectionName("Command Summary")]
        [FunctionName("GetCommandStatusInformationProjectionActivity")]
        public static async Task<Command_Summary_Projection_Return> GetCommandStatusInformationProjectionActivity(
                [ActivityTrigger] DurableActivityContext context,
                ILogger log
                )
        {

            Command_Summary_Projection_Request commandInfo = context.GetInput<Command_Summary_Projection_Request>();

            if (null != log)
            {
                log.LogInformation($"GetCommandStatusInformationProjectionActivity called for command : {commandInfo.CommandName } - ID: {commandInfo.UniqueIdentifier }");
            }

            return await ProcessCommandStatusInformationProjection(commandInfo.CommandName ,
                commandInfo.UniqueIdentifier ,
                log);
        }


            private static async Task<Command_Summary_Projection_Return> ProcessCommandStatusInformationProjection(
                string commandId,
                string commandName,
                ILogger log)
        { 

            Command_Summary_Projection_Return ret = null;
            Guid commandGuid;

            // use custom assembly resolve handler
            using (new AzureFunctionsResolveAssembly())
            {
                if (Guid.TryParse(commandId, out commandGuid))
                {
                    #region Logging
                    if (null != log)
                    {
                        log.LogDebug($"Validating command {commandId} in HandleCreateLeagueCommand");
                    }
                    #endregion

                    // Get the current state of the command...
                    Projection getCommandState = new Projection(@"Command",
                        commandName,
                        commandGuid.ToString(),
                        nameof(Command_Summary_Projection));

                    if (null != getCommandState)
                    {

                        #region Logging
                        if (null != log)
                        {
                            log.LogDebug($"Projection processor created in HandleCreateLeagueCommand");
                        }
                        #endregion

                        Command_Summary_Projection cmdProjection =
                            new Command_Summary_Projection(log);

                        await getCommandState.Process(cmdProjection);

                        if ((cmdProjection.CurrentSequenceNumber > 0) || (cmdProjection.ProjectionValuesChanged()))
                        {
                            ret = new Command_Summary_Projection_Return() {
                                AsOfDate = cmdProjection.CurrentAsOfDate ,
                                AsOfStepNumber = (int)cmdProjection.CurrentSequenceNumber ,
                                Status = cmdProjection.CurrentState.ToString() ,
                                CommandName = commandName ,
                                CorrelationIdentifier = cmdProjection.CorrelationIdentifier 
                            };
                        }
                    }

                }
            }

            return ret;
        }
    }
}
