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

namespace TheLongRunLeaguesFunction.Commands.Validation
{
    public static partial class Command
    {

        [ApplicationName("The Long Run")]
        [DomainName("Leagues")]
        [AggregateRoot("League")]
        [CommandName("Set League Email Address")]
        [FunctionName("SetLeagueEmailAddressCommandValidation")]
        public static async Task<HttpResponseMessage> SetLeagueEmailAddressCommandValidationRun(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequestMessage req, 
            TraceWriter log)
        {
            #region Logging
            if (null != log)
            {
                log.Verbose("Function triggered HTTP ",
                    source: "SetLeagueEmailAddressCommandValidation");
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
            if (ValidateSetLeagueEmailAddressCommand(commandId, log))
            {
                valid = true;

#if FUNCTION_CHAINING
                // Call the next command in the command chain to process the valid command
                FunctionChaining funcChain = new FunctionChaining(log);
                var queryParams = new System.Collections.Generic.List<Tuple<string, string>>();
                queryParams.Add(new Tuple<string, string>("commandId", commandId.ToString()));
                funcChain.TriggerCommandByHTTPS(@"Leagues", "SetLeagueEmailAddressCommandHandler", queryParams, null);
#endif

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
        public static bool SetLeagueEmailAddressCommandValidationActivity(
            [ActivityTrigger] DurableActivityContext setLeagueEmailAddressCommandContect,
            TraceWriter log)
        {

            // Return the validation result
            string commandId = setLeagueEmailAddressCommandContect.GetInput<string>();
            if (! string.IsNullOrWhiteSpace(commandId ) )
            {
                return ValidateSetLeagueEmailAddressCommand(commandId, log);
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
        private static bool  ValidateSetLeagueEmailAddressCommand(string commandId, 
            TraceWriter log)
        {

            const string COMMAND_NAME = @"set-league-email-address";

            Guid commandGuid;

            if (Guid.TryParse(commandId, out commandGuid))
            {
#region Logging
                if (null != log)
                {
                    log.Verbose($"Validating command {commandId} ",
                        source: "ValidateSetLeagueEmailAddressCommand");
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
                        log.Verbose($"Projection processor created",
                            source: "ValidateSetLeagueEmailAddressCommand");
                    }
#endregion

                    Command_Summary_Projection cmdProjection =
                        new Command_Summary_Projection(log);

                    getCommandState.Process(cmdProjection);

                    if ((cmdProjection.CurrentSequenceNumber > 0) || (cmdProjection.ProjectionValuesChanged()))
                    {

#region Logging
                        if (null != log)
                        {
                            log.Verbose($"Command { cmdProjection.CommandName} projection run for {commandGuid} ",
                                source: "ValidateSetLeagueEmailAddressCommand");
                        }
#endregion

                        if (cmdProjection.CurrentState ==
                            Command_Summary_Projection.CommandState.Completed)
                        {
                            // No need to validate a completed command
#region Logging
                            if (null != log)
                            {
                                log.Warning($"Command {commandId} is complete so no need to validate ",
                                    source: "ValidateSetLeagueEmailAddressCommand");
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
                                log.Warning($"Command {commandId} is validated so no need to validate again ",
                                    source: "ValidateSetLeagueEmailAddressCommand");
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
                                    CommandErrorLogRecord.LogCommandValidationError(commandGuid, COMMAND_NAME, true, "League name may not be blank");
#region Logging
                                    if (null != log)
                                    {
                                        log.Warning($"Command {COMMAND_NAME } :: {commandId} has a blank league name",
                                            source: "ValidateSetLeagueEmailAddressCommand");
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
                                    CommandErrorLogRecord.LogCommandValidationError(commandGuid, COMMAND_NAME, false, "Email address is blank");
#region Logging
                                    if (null != log)
                                    {
                                        log.Warning($"Command {COMMAND_NAME } :: {commandId} has a future dated incorporation date",
                                            source: "ValidateSetLeagueEmailAddressCommand");
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
                                CommandErrorLogRecord.LogCommandValidationSuccess(commandGuid, COMMAND_NAME);
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
                            log.Warning($"No command events read for {commandId} ",
                                source: "ValidateSetLeagueEmailAddressCommand");
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
