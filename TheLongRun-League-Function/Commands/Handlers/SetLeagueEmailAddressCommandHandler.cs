using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Linq;
using Leagues.League.commandDefinition;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;



using Newtonsoft.Json;
using TheLongRun.Common;
using TheLongRun.Common.Attributes;
using TheLongRun.Common.Bindings;
using TheLongRun.Common.Events.Command.Projections;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace TheLongRunLeaguesFunction.Commands.Handlers
{
    public static partial class CommandHandler
    {

        [ApplicationName("The Long Run")]
        [DomainName("Leagues")]
        [AggregateRoot("League")]
        [CommandName("Set League Email Address")]
        [FunctionName("SetLeagueEmailAddressCommandHandler")]
        public static async Task<HttpResponseMessage> SetLeagueEmailAddressCommandHandlerRun(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequestMessage req,
            TraceWriter log)
        {

            #region Logging
            if (null != log)
            {
                log.Verbose("Function triggered HTTP ",
                    source: "CreateLeagueCommandHandler");
            }
            #endregion

            // Get the command identifier
            string commandId = req.GetQueryNameValuePairs()[@"CommandId"];

            if (commandId == null)
            {
                // Get request body
                dynamic data = await req.Content.ReadAsAsync<object>();
                commandId = data?.CommandId;
            }

            HandleSetLeagueEmailAddressCommand(commandId, log);

            return commandId == null
                ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a commandId on the query string or in the request body")
                : req.CreateResponse(HttpStatusCode.OK, $"Handled command {commandId}");
        }


        [ApplicationName("The Long Run")]
        [DomainName("Leagues")]
        [AggregateRoot("League")]
        [CommandName("Set League Email Address")]
        [FunctionName("SetLeagueEmailAddressCommandHandlerActivity")]
        public static  void SetLeagueEmailAddressCommandHandlerActivity(
            [ActivityTrigger] DurableActivityContext setLeagueEmailAddressCommandContect,
            TraceWriter log)
        {

            string commandId = setLeagueEmailAddressCommandContect.GetInput<string>();
            if (!string.IsNullOrWhiteSpace(commandId))
            {
                 HandleSetLeagueEmailAddressCommand(commandId, log);
            }

            return;
        }

        /// <summary>
        /// Perform the underlying processing on the specified command
        /// </summary>
        /// <param name="commandId">
        /// The unique identifier of the command to process
        /// </param>
        private static void HandleSetLeagueEmailAddressCommand(string commandId,
            TraceWriter log = null)
        {

            const string COMMAND_NAME = @"set-league-email-address";

            Guid commandGuid;

            if (Guid.TryParse(commandId, out commandGuid))
            {
                #region Logging
                if (null != log)
                {
                    log.Verbose($"Validating command {commandId} ",
                        source: "HandleSetLeagueEmailAddressCommand");
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
                            source: "HandleSetLeagueEmailAddressCommand");
                    }
                    #endregion

                    Command_Summary_Projection cmdProjection =
                        new Command_Summary_Projection(log);

                    getCommandState.Process(cmdProjection);

                    if ((cmdProjection.CurrentSequenceNumber > 0) || (cmdProjection.ProjectionValuesChanged()))
                    {
                        if (cmdProjection.CurrentState == Command_Summary_Projection.CommandState.Completed)
                        {
                            // No need to process a completed projection
                            #region Logging
                            if (null != log)
                            {
                                log.Warning($"Command {commandId} is complete so no need to process ",
                                    source: "HandleSetLeagueEmailAddressCommand");
                            }
                            #endregion
                            return;
                        }

                        if (cmdProjection.CurrentState == Command_Summary_Projection.CommandState.Created)
                        {
                            // No need to process a completed projection
                            #region Logging
                            if (null != log)
                            {
                                log.Warning($"Command {commandId} is not yet validated so cannot process ",
                                    source: "HandleSetLeagueEmailAddressCommand");
                            }
                            #endregion
                            return;
                        }

                        if (cmdProjection.CurrentState == Command_Summary_Projection.CommandState.Invalid)
                        {
                            // No need to process a completed projection
                            #region Logging
                            if (null != log)
                            {
                                log.Warning($"Command {commandId} is not yet marked as invalid so cannot process ",
                                    source: "HandleSetLeagueEmailAddressCommand");
                            }
                            #endregion
                            return;
                        }

                        if (cmdProjection.CurrentState == Command_Summary_Projection.CommandState.Validated)
                        {

                            string leagueName = string.Empty;

                            // New or previously invalid command can be validated
                            if (cmdProjection.ParameterIsSet(nameof(Set_Email_Address_Definition.LeagueName)))
                            {
                                // Get the league name we want to set the email address 
                                leagueName = cmdProjection.GetParameter<string>(nameof(Set_Email_Address_Definition.LeagueName));
                            }

                            string twitterHandle = string.Empty;
                            if (cmdProjection.ParameterIsSet(nameof(Create_New_League_Definition.Twitter_Handle)))
                            {
                                twitterHandle = cmdProjection.GetParameter<string>(nameof(Create_New_League_Definition.Twitter_Handle));
                            }

                            string emailAddress = string.Empty;
                            if (cmdProjection.ParameterIsSet(nameof(Set_Email_Address_Definition.New_Email_Address)))
                            {
                                emailAddress = cmdProjection.GetParameter<string>(nameof(Set_Email_Address_Definition.New_Email_Address ));
                            }

                            string notes = string.Empty;
                            if (cmdProjection.ParameterIsSet(nameof(Set_Email_Address_Definition.Notes )))
                            {
                                notes = cmdProjection.GetParameter<string>(nameof(Set_Email_Address_Definition.Notes ));
                            }

                            // Create a new "Contact Details Changed" event
                            Leagues.League.eventDefinition.Contact_Details_Changed changedEvent = new Leagues.League.eventDefinition.Contact_Details_Changed(DateTime.UtcNow,
                                twitterHandle,
                                emailAddress);

                            EventStream leagueEvents = new EventStream(@"Leagues",
                                        "League",
                                        leagueName);

                            if ((null != leagueEvents) && (null != changedEvent))
                            {
                                leagueEvents.AppendEvent(changedEvent);
                            }


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
                            source: "HandleSetLeagueEmailAddressCommand");
                    }
                    #endregion
                }
            }
        }
    }
}
