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
using TheLongRun.Common.Orchestration;

namespace TheLongRunLeaguesFunction.Commands.Validation
{
    public static partial class SetLeagueEmailAddressCommandHandler
    {

        [ApplicationName("The Long Run")]
        [DomainName("Leagues")]
        [AggregateRoot("League")]
        [CommandName("Set League Email Address")]
        [FunctionName("SetLeagueEmailAddressCommandValidation")]
        public static async Task<HttpResponseMessage> SetLeagueEmailAddressCommandValidationRun(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequestMessage req, 
            ILogger log)
        {
            #region Logging
            if (null != log)
            {
                log.LogDebug("Function triggered HTTP in SetLeagueEmailAddressCommandValidation");
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

            bool valid = false;
            if (await ValidateSetLeagueEmailAddressCommand(commandId, log))
            {
                valid = true;
            }

            if (null == commandId )
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a commandId on the query string or in the request body");
            }
            else
            {
                if (valid )
                {
                    return req.CreateResponse(HttpStatusCode.OK, $"Validation for command {commandId} succeeded");
                }
                else
                {
                    return req.CreateResponse(HttpStatusCode.Forbidden , $"Validation for command {commandId} failed");
                }
            }

        }


        [ApplicationName("The Long Run")]
        [DomainName("Leagues")]
        [AggregateRoot("League")]
        [CommandName("Set League Email Address")]
        [FunctionName("SetLeagueEmailAddressCommandValidationActivity")]
        public static async Task<bool> SetLeagueEmailAddressCommandValidationActivity(
            [ActivityTrigger] DurableActivityContext setLeagueEmailAddressCommandContect,
            ILogger log)
        {

            // Return the validation result
            string commandId = setLeagueEmailAddressCommandContect.GetInput<string>();
            if (! string.IsNullOrWhiteSpace(commandId ) )
            {
                return await ValidateSetLeagueEmailAddressCommand(commandId, log,
                     new WriteContext("SetLeagueEmailAddressCommandValidationActivity", setLeagueEmailAddressCommandContect.InstanceId ));
            }

            return false;
        }



        /// <summary>
        /// Perform the underlying validation on the specified command
        /// </summary>
        /// <param name="commandId">
        /// The unique identifier of the command to validate
        /// </param>
        /// <remarks>
        /// This is common functionality, whether called by function chaining or by duarble functions
        /// </remarks>
        private static async Task<bool>  ValidateSetLeagueEmailAddressCommand(string commandId, 
            ILogger log,
            CQRSAzure.EventSourcing.IWriteContext writeContext =null )
        {

            const string COMMAND_NAME = @"set-league-email-address";

            Guid commandGuid;

            if (Guid.TryParse(commandId, out commandGuid))
            {
#region Logging
                if (null != log)
                {
                    log.LogDebug($"Validating command {commandId} in ValidateSetLeagueEmailAddressCommand");
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
                        log.LogDebug($"Projection processor created in ValidateSetLeagueEmailAddressCommand");
                    }
#endregion

                    Command_Summary_Projection cmdProjection =
                        new Command_Summary_Projection(log);

                    await getCommandState.Process(cmdProjection);

                    if ((cmdProjection.CurrentSequenceNumber > 0) || (cmdProjection.ProjectionValuesChanged()))
                    {

#region Logging
                        if (null != log)
                        {
                            log.LogDebug($"Command { cmdProjection.CommandName} projection run for {commandGuid} in ValidateSetLeagueEmailAddressCommand");
                        }
#endregion

                        if (cmdProjection.CurrentState ==
                            Command_Summary_Projection.CommandState.Completed)
                        {
                            // No need to validate a completed command
#region Logging
                            if (null != log)
                            {
                                log.LogWarning($"Command {commandId} is complete so no need to validate in ValidateSetLeagueEmailAddressCommand");
                            }
#endregion
                            return true ;
                        }

                        if (cmdProjection.CurrentState ==
                            Command_Summary_Projection.CommandState.Validated)
                        {
                            // No need to process ana lready validated command
#region Logging
                            if (null != log)
                            {
                                log.LogWarning($"Command {commandId} is validated so no need to validate again in ValidateSetLeagueEmailAddressCommand");
                            }
#endregion
                            return true;
                        }

                        if ((cmdProjection.CurrentState ==
                            Command_Summary_Projection.CommandState.Created) ||
                            (cmdProjection.CurrentState ==
                            Command_Summary_Projection.CommandState.Invalid))
                        {

                            bool leagueNameValid = false;
                            bool emailAddressValid = false;

                            // New or previously invalid command can be validated
                            if (cmdProjection.ParameterIsSet(nameof(Set_Email_Address_Definition.LeagueName)))
                            {
                                // League name may not be blank
                                string leagueName = cmdProjection.GetParameter<string>(nameof(Create_New_League_Definition.LeagueName));
                                if (string.IsNullOrWhiteSpace(leagueName))
                                {
                                    await CommandErrorLogRecord.LogCommandValidationError(commandGuid, COMMAND_NAME, true, "League name may not be blank",
                                        writeContext );
#region Logging
                                    if (null != log)
                                    {
                                        log.LogWarning($"Command {COMMAND_NAME } :: {commandId} has a blank league name in ValidateSetLeagueEmailAddressCommand");
                                    }
#endregion
                                }
                                else
                                {
                                    leagueNameValid = true;
                                }
                            }

                            // The email address should not be blank
                            if (cmdProjection.ParameterIsSet(nameof(Create_New_League_Definition.Email_Address )))
                            {
                                string emailAddress = cmdProjection.GetParameter<string >(nameof(Create_New_League_Definition.Email_Address ));
                                if (string.IsNullOrEmpty(emailAddress) )
                                {
                                   await  CommandErrorLogRecord.LogCommandValidationError(commandGuid, COMMAND_NAME, false, "Email address is blank",
                                       writeContext );
#region Logging
                                    if (null != log)
                                    {
                                        log.LogWarning($"Command {COMMAND_NAME } :: {commandId} has a future dated incorporation date in ValidateSetLeagueEmailAddressCommand");
                                    }
#endregion
                                }
                                else
                                {
                                    emailAddressValid = true;
                                }
                            }

                            if (emailAddressValid && leagueNameValid)
                            {
                                await CommandErrorLogRecord.LogCommandValidationSuccess(commandGuid, COMMAND_NAME,
                                    writeContext );
                                return true;
                            }
                        }
                    }
                    else
                    {
                        // No events were read into the projection so do nothing
#region Logging
                        if (null != log)
                        {
                            log.LogWarning($"No command events read for {commandId} in ValidateSetLeagueEmailAddressCommand");
                        }
#endregion
                    }
                }
            }

            // If, for any reason, we can't validate the command the default is to return false
            return false;
        }
    }
}
