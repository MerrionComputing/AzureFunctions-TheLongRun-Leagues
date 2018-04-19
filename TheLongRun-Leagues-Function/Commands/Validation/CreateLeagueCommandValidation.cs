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
    /// <summary>
    /// Validation to be performed on the [Create-New-League] command
    /// </summary>
    /// <remarks>
    /// League name may not be empty
    /// League name may not be duplicate
    /// Date_Incorporated may not be future dated
    /// </remarks>
    public static partial class Command
    {
        [ApplicationName("The Long Run")]
        [DomainName("Leagues")]
        [AggregateRoot("League")]
        [CommandName("Create League")]
        [FunctionName("CreateLeagueCommandValidation")]
        public static async Task<HttpResponseMessage> CreateLeagueCommandValidationRun(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequestMessage req, 
            TraceWriter log)
        {

 

                #region Logging
                if (null != log)
                {
                    log.Verbose("Function triggered HTTP ",
                        source: "CreateLeagueCommandValidation");
                }
                #endregion

                // Get the command identifier
                string commandId = req.GetQueryNameValuePairs()
                    .FirstOrDefault(q => string.Compare(q.Key, "CommandId", true) == 0)
                    .Value;

                if (commandId == null)
                {
                    // Get request body
                    dynamic data = await req.Content.ReadAsAsync<object>();
                    commandId = data?.CommandId;
                }

                ValidateCreateLeagueCommand(commandId, log);

                return commandId == null
                    ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a commandId on the query string or in the request body")
                    : req.CreateResponse(HttpStatusCode.OK, $"Validated command {commandId}");
        }

        /// <summary>
        /// Perform the underlying validation on the specified command
        /// </summary>
        /// <param name="commandId">
        /// The unique identifier of the command to validate
        /// </param>
        private static void ValidateCreateLeagueCommand(string commandId,
            TraceWriter log = null)
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
                        log.Verbose($"Validating command {commandId} ",
                            source: "ValidateCreateLeagueCommand");
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
                                source: "ValidateCreateLeagueCommand");
                        }
                        #endregion

                        Command_Summary_Projection cmdProjection =
                            new Command_Summary_Projection(log );

                        getCommandState.Process(cmdProjection);

                        if ( (cmdProjection.CurrentSequenceNumber > 0) || (cmdProjection.ProjectionValuesChanged()))
                        {

                            #region Logging
                            if (null != log)
                            {
                                log.Verbose ($"Command { cmdProjection.CommandName} projection run for {commandGuid} ",
                                    source: "ValidateCreateLeagueCommand");
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
                                        source: "ValidateCreateLeagueCommand");
                                }
                                #endregion
                                return;
                            }

                            if (cmdProjection.CurrentState ==
                                Command_Summary_Projection.CommandState.Validated)
                            {
                                // No need to process a completed projection
                                #region Logging
                                if (null != log)
                                {
                                    log.Warning($"Command {commandId} is validated so no need to validate again ",
                                        source: "ValidateCreateLeagueCommand");
                                }
                                #endregion
                                return;
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
                                        CommandErrorLogRecord.LogCommandValidationError(commandGuid, COMMAND_NAME, true, "League name may not be blank");
                                        #region Logging
                                        if (null != log)
                                        {
                                            log.Warning($"Command {COMMAND_NAME } :: {commandId} has a blank league name",
                                                source: "ValidateCreateLeagueCommand");
                                        }
                                        #endregion
                                    }
                                    else
                                    {
                                        leagueNameValid = true;
                                    }
                                }

                                // The incoporation date may not be in the future
                                if (cmdProjection.ParameterIsSet(nameof(Create_New_League_Definition.Date_Incorporated)))
                                {
                                    DateTime dateIncorporated = cmdProjection.GetParameter<DateTime>(nameof(Create_New_League_Definition.Date_Incorporated));
                                    if (dateIncorporated > DateTime.UtcNow)
                                    {
                                        CommandErrorLogRecord.LogCommandValidationError(commandGuid, COMMAND_NAME, false, "Incorporation date is in the future");
                                        #region Logging
                                        if (null != log)
                                        {
                                            log.Warning($"Command {COMMAND_NAME } :: {commandId} has a future dated incorporation date",
                                                source: "ValidateCreateLeagueCommand");
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
                                    CommandErrorLogRecord.LogCommandValidationSuccess(commandGuid, COMMAND_NAME);

                                    // Call the next command in the command chain to process the valid command
                                    FunctionChaining funcChain = new FunctionChaining(log);
                                    var queryParams = new System.Collections.Generic.List<Tuple<string, string>>();
                                    queryParams.Add(new Tuple<string, string>("commandId", commandGuid.ToString()));
                                    funcChain.TriggerCommandByHTTPS(@"Leagues", "CreateLeagueCommandHandler", queryParams, null);
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
                                    source: "ValidateCreateLeagueCommand");
                            }
                            #endregion
                        }
                    }
                }
            }
        }
    }
}
