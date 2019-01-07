using CQRSAzure.EventSourcing;
using Leagues.League.commandDefinition;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using TheLongRun.Common;
using TheLongRun.Common.Attributes;
using TheLongRun.Common.Bindings;
using TheLongRun.Common.Events.Command.Projections;
using TheLongRun.Common.Orchestration;

namespace TheLongRunLeaguesFunction.Commands.Handlers
{
    public static partial class SetLeagueEmailAddressCommandHandler
    {

        [ApplicationName("The Long Run")]
        [TheLongRun.Common.Attributes.DomainName("Leagues")]
        [AggregateRoot("League")]
        [CommandName("Set League Email Address")]
        [FunctionName("SetLeagueEmailAddressCommandHandler")]
        public static async Task<HttpResponseMessage> SetLeagueEmailAddressCommandHandlerRun(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequestMessage req,
            ILogger log)
        {

            #region Logging
            if (null != log)
            {
                log.LogDebug("Function triggered HTTP in CreateLeagueCommandHandler");
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

            await HandleSetLeagueEmailAddressCommand(commandId, log);

            return commandId == null
                ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a commandId on the query string or in the request body")
                : req.CreateResponse(HttpStatusCode.OK, $"Handled command {commandId}");
        }


        [ApplicationName("The Long Run")]
        [TheLongRun.Common.Attributes.DomainName("Leagues")]
        [AggregateRoot("League")]
        [CommandName("Set League Email Address")]
        [FunctionName("SetLeagueEmailAddressCommandHandlerActivity")]
        public static  async Task<ActivityResponse> SetLeagueEmailAddressCommandHandlerActivity(
            [ActivityTrigger] DurableActivityContext context,
            ILogger log)
        {

            ActivityResponse ret = new ActivityResponse() { FunctionName = "SetLeagueEmailAddressCommandHandlerActivity" };

            try
            {
                CommandRequest<Set_Email_Address_Definition > cmdRequest = context.GetInput<CommandRequest<Set_Email_Address_Definition>>();

                if (null != cmdRequest)
                {
                    await HandleSetLeagueEmailAddressCommand(cmdRequest.CommandUniqueIdentifier.ToString() , log,
                        new WriteContext(ret.FunctionName , context.InstanceId ) );
                    ret.Message = $"Command processed {cmdRequest.CommandName} : {cmdRequest.CommandUniqueIdentifier}";
                }
                else
                {
                    ret.FatalError = true;
                    ret.Message = $"Unable to read command request from {context.InstanceId }";
                }
            }
            catch (Exception ex)
            {
                ret.FatalError = true;
                ret.Message = ex.Message;
            }

            return ret;
        }

        /// <summary>
        /// Perform the underlying processing on the specified command
        /// </summary>
        /// <param name="commandId">
        /// The unique identifier of the command to process
        /// </param>
        private static async Task  HandleSetLeagueEmailAddressCommand(string commandId,
            ILogger log,
            IWriteContext writeContext = null)
        {

            const string COMMAND_NAME = @"set-league-email-address";

            Guid commandGuid;

            if (Guid.TryParse(commandId, out commandGuid))
            {
                #region Logging
                if (null != log)
                {
                    log.LogDebug($"Validating command {commandId}  in HandleSetLeagueEmailAddressCommand");
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
                        log.LogDebug($"Projection processor created in HandleSetLeagueEmailAddressCommand");
                    }
                    #endregion

                    Command_Summary_Projection cmdProjection =
                        new Command_Summary_Projection(log);

                    await getCommandState.Process(cmdProjection);

                    if ((cmdProjection.CurrentSequenceNumber > 0) || (cmdProjection.ProjectionValuesChanged()))
                    {
                        if (cmdProjection.CurrentState == Command_Summary_Projection.CommandState.Completed)
                        {
                            // No need to process a completed projection
                            #region Logging
                            if (null != log)
                            {
                                log.LogWarning($"Command {commandId} is complete so no need to process in HandleSetLeagueEmailAddressCommand");
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
                                log.LogWarning($"Command {commandId} is not yet validated so cannot process in HandleSetLeagueEmailAddressCommand");
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
                                log.LogWarning($"Command {commandId} is not yet marked as invalid so cannot process in HandleSetLeagueEmailAddressCommand");
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
                                if (null != writeContext )
                                {
                                    leagueEvents.SetContext(writeContext);
                                }
                                await leagueEvents.AppendEvent(changedEvent);
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
                        log.LogWarning($"No command events read for {commandId} in HandleSetLeagueEmailAddressCommand");
                    }
                    #endregion
                }
            }
        }
    }
}
