using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

using TheLongRun.Common.Events.Command.Projections;
using TheLongRun.Common.Bindings;
using System;
using Leagues.League.commandDefinition;
using TheLongRun.Common;
using TheLongRun.Common.Attributes;
using Microsoft.Extensions.Logging;

namespace TheLongRunLeaguesFunction.Commands.Validation
{
    /// <summary>
    /// Validation to be performed on the [Create-New-League] command
    /// </summary>
    /// <remarks>
    /// League name may not be empty
    /// League name may not be duplicate
    /// Date_Incorporated may not be future dated
    /// </remarks>
    public static partial class CreateLeagueCommandHandler
    {
        [ApplicationName("The Long Run")]
        [DomainName("Leagues")]
        [AggregateRoot("League")]
        [CommandName("Create League")]
        [FunctionName("CreateLeagueCommandValidation")]
        public static async Task<HttpResponseMessage> CreateLeagueCommandValidationRun(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequestMessage req, 
            ILogger log)
        {

                #region Logging
                if (null != log)
                {
                    log.LogDebug("Function triggered HTTP in CreateLeagueCommandValidation");
                }
            #endregion

            // Get the command identifier
            string commandId = req.GetQueryNameValuePairsExt()[@"CommandId"];

                if (commandId == null)
                {
                    // Get request body
                    dynamic data = await req.Content.ReadAsAsync<object>();
                    commandId = data?.CommandId;
                }

               bool valid  = await ValidateCreateLeagueCommand(commandId, log);

                return commandId == null
                    ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a commandId on the query string or in the request body")
                    : req.CreateResponse(HttpStatusCode.OK, $"Validated command {commandId} : {valid}");
        }


        [ApplicationName("The Long Run")]
        [DomainName("Leagues")]
        [AggregateRoot("League")]
        [CommandName("Create League")]
        [FunctionName("CreateLeagueCommandValidationAction")]
        public static async Task<bool> CreateLeagueCommandValidationAction
            ([ActivityTrigger] CommandRequest<Create_New_League_Definition > cmdRequest,
            ILogger log)
        {

            if (null != log)
            {
                log.LogInformation($"CreateLeagueCommandValidationAction called for command : {cmdRequest.CommandUniqueIdentifier}");
            }

            return await ValidateCreateLeagueCommand(cmdRequest.CommandUniqueIdentifier.ToString(), log);
        }

        /// <summary>
        /// Perform the underlying validation on the specified command
        /// </summary>
        /// <param name="commandId">
        /// The unique identifier of the command to validate
        /// </param>
        private static async Task<bool> ValidateCreateLeagueCommand(string commandId,
            ILogger log = null)
        {

            const string COMMAND_NAME = @"create-league";

            Guid commandGuid;

            // use custom assembly resolve handler
            using (new AzureFunctionsResolveAssembly())
            {
                if (Guid.TryParse(commandId, out commandGuid))
                {
                    #region Logging
                    if (null != log)
                    {
                        log.LogDebug($"Validating command {commandId} in ValidateCreateLeagueCommand");
                    }
                    #endregion

                    // Get the current state of the command...
                    Projection getCommandState = new Projection(@"Command",
                        COMMAND_NAME,
                        commandGuid.ToString(),
                        nameof(Command_Summary_Projection));

                    if (null != getCommandState)
                    {

                        #region Logging
                        if (null != log)
                        {
                            log.LogDebug($"Projection processor created in ValidateCreateLeagueCommand");
                        }
                        #endregion

                        Command_Summary_Projection cmdProjection =
                            new Command_Summary_Projection(log );

                        await getCommandState.Process(cmdProjection);

                        if ( (cmdProjection.CurrentSequenceNumber > 0) || (cmdProjection.ProjectionValuesChanged()))
                        {

                            #region Logging
                            if (null != log)
                            {
                                log.LogDebug ($"Command { cmdProjection.CommandName} projection run for {commandGuid} in ValidateCreateLeagueCommand");
                            }
                            #endregion

                            if (cmdProjection.CurrentState ==
                                Command_Summary_Projection.CommandState.Completed)
                            {
                                // No need to validate a completed command
                                #region Logging
                                if (null != log)
                                {
                                    log.LogWarning($"Command {commandId} is complete so no need to validate in ValidateCreateLeagueCommand");
                                }
                                #endregion
                                return true ;
                            }

                            if (cmdProjection.CurrentState ==
                                Command_Summary_Projection.CommandState.Validated)
                            {
                                // No need to process a completed projection
                                #region Logging
                                if (null != log)
                                {
                                    log.LogWarning($"Command {commandId} is validated so no need to validate again in ValidateCreateLeagueCommand");
                                }
                                #endregion
                                return true ;
                            }

                            if ((cmdProjection.CurrentState ==
                                Command_Summary_Projection.CommandState.Created) ||
                                (cmdProjection.CurrentState ==
                                Command_Summary_Projection.CommandState.Invalid))
                            {

                                bool leagueNameValid = false;
                                bool incoporatedDateValid = false;

                                // New or previously invalid command can be validated
                                if (cmdProjection.ParameterIsSet(nameof(Create_New_League_Definition.LeagueName)))
                                {
                                    // League name may not be blank
                                    string leagueName = cmdProjection.GetParameter<string>(nameof(Create_New_League_Definition.LeagueName));
                                    if (string.IsNullOrWhiteSpace(leagueName))
                                    {
                                        await CommandErrorLogRecord.LogCommandValidationError(commandGuid, COMMAND_NAME, true, "League name may not be blank");
                                        #region Logging
                                        if (null != log)
                                        {
                                            log.LogWarning($"Command {COMMAND_NAME } :: {commandId} has a blank league name in ValidateCreateLeagueCommand");
                                        }
                                        #endregion
                                    }
                                    else
                                    {
                                        leagueNameValid = true;
                                    }
                                }

                                // The incoporation date may not be in the future
                                if (cmdProjection.ParameterIsSet(nameof(ICreate_New_League_Definition.Date_Incorporated)))
                                {
                                    DateTime dateIncorporated = cmdProjection.GetParameter<DateTime>(nameof(ICreate_New_League_Definition.Date_Incorporated));
                                    if (dateIncorporated > DateTime.UtcNow)
                                    {
                                        await CommandErrorLogRecord.LogCommandValidationError(commandGuid, COMMAND_NAME, false, "Incorporation date is in the future");
                                        #region Logging
                                        if (null != log)
                                        {
                                            log.LogWarning($"Command {COMMAND_NAME } :: {commandId} has a future dated incorporation date in ValidateCreateLeagueCommand");
                                        }
                                        #endregion
                                    }
                                    else
                                    {
                                        incoporatedDateValid = true;
                                    }
                                }

                                if (incoporatedDateValid && leagueNameValid)
                                {
                                    await CommandErrorLogRecord.LogCommandValidationSuccess(commandGuid, COMMAND_NAME);
                                }

                                return (incoporatedDateValid && leagueNameValid);
                            }
                        }
                        else
                        {
                            // No events were read into the projection so do nothing
#region Logging
                            if (null != log)
                            {
                                log.LogWarning($"No command events read for {commandId} in ValidateCreateLeagueCommand");
                            }
#endregion
                        }
                    }
                }

                return false;
            }
        }
    }
}
