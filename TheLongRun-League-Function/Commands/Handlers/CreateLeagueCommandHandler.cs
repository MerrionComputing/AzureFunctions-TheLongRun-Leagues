using System;
using System.Net;
using System.Net.Http;
using System.Linq;
using Leagues.League.commandDefinition;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

using TheLongRun.Common;
using TheLongRun.Common.Attributes;
using TheLongRun.Common.Bindings;
using TheLongRun.Common.Events.Command.Projections;
using System.Threading.Tasks;

using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace TheLongRunLeaguesFunction.Commands.Handlers
{
    public static partial class CommandHandler
    {


#if FILE_TRIGGER
        /// <summary>
        /// Command handler to handle the [create-league] command
        /// </summary>
        /// <param name="myBlob">
        /// The file containing the command context and parameters
        /// </param>
        /// <param name="name">
        /// The name of the file (should this need to be logged or copied/moved)
        /// </param>
        /// <param name="log">
        /// Log output to trace processing with
        /// </param>
        /// <remarks>
        /// A create league command reaching this record is assumed to be valid
        /// </remarks>
        [ApplicationName("The Long Run")]
        [DomainName("Leagues")]
        [AggregateRoot("League")]
        [CommandName("Create League")]
        [FunctionName("CreateLeagueCommandHandler")]
        public static void CreateLeagueCommandHandler(
            [BlobTrigger("command-log/create-league/{name}", 
            Connection = "CommandStorageConnectionAppSetting")]Stream  myBlob, 
            string name,
            ILogger log)
        {

#region Logging
            if (null != log)
            {
                log.LogInfo($"Command handler for [create-league] command called\n Payload file name:{name} \n Size: {myBlob.Length} Bytes", 
                    source: "CreateLeagueCommandHandler");
            }
#endregion

            // Handle the command
            if (null != myBlob)
            {
                // The file will contain a CommandLogRecord<Create_New_League_Definition>()
                CommandLogRecord<Create_New_League_Definition> cmdRecord = null;

                var serializer = new JsonSerializer();

                using (var sr = new StreamReader(myBlob))
                {
                    using (var jsonTextReader = new JsonTextReader(sr))
                    {
                        cmdRecord = serializer.Deserialize<CommandLogRecord<Create_New_League_Definition>>(jsonTextReader); 
                    }
                }
                
                if (null != cmdRecord )
                {
#region Logging
                    if (null != log)
                    {
                        log.LogInfo($"Command read from file : {cmdRecord.CommandName} , Id: {cmdRecord.CommandUniqueIdentifier } issued at {cmdRecord.When } by {cmdRecord.Who } ",
                            source: "CreateLeagueCommandHandler");
                    }
#endregion

                    // Post an "League incorporated" event
                    EventStream outputStream = new EventStream("Leagues", "League", cmdRecord.Parameters.LeagueName);
                    if (null != outputStream)
                    {
                        // create a new "League created" event
                        Leagues.League.eventDefinition.Formed evtFormed = new Leagues.League.eventDefinition.Formed(cmdRecord.Parameters.Date_Incorporated,
                            cmdRecord.Parameters.Location,
                            $"League : {cmdRecord.Parameters.LeagueName} contactable by email {cmdRecord.Parameters.Email_Address} or twitter {cmdRecord.Parameters.Twitter_Handle } ");

                        outputStream.AppendEvent(evtFormed); 
                    }

                }
            }
            else
            {
#region Logging
                if (null != log)
                {
                    log.Error($"Empty command log passed to [create-league] command in {name}");
                }
#endregion
            }


            // Log completion
            if (null != log)
            {
                log.LogDebug ($"Command handler for [create-league] command completed",
                    source: "CreateLeagueCommandHandler");
            }


        }

#endif 

        [ApplicationName("The Long Run")]
        [DomainName("Leagues")]
        [AggregateRoot("League")]
        [CommandName("Create League")]
        [FunctionName("CreateLeagueCommandHandler")]
        public static async Task<HttpResponseMessage> CreateLeagueCommandHandlerRun(
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

            await HandleCreateLeagueCommand(commandId, log);

            return commandId == null
                ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a commandId on the query string or in the request body")
                : req.CreateResponse(HttpStatusCode.OK, $"Handled command {commandId}");
        }

        /// <summary>
        /// Perform the underlying processing on the specified command
        /// </summary>
        /// <param name="commandId">
        /// The unique identifier of the command to process
        /// </param>
        private static async Task HandleCreateLeagueCommand(string commandId,
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
                        log.LogDebug($"Validating command {commandId} in HandleCreateLeagueCommand");
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
                            log.LogDebug($"Projection processor created in HandleCreateLeagueCommand");
                        }
#endregion

                        Command_Summary_Projection cmdProjection =
                            new Command_Summary_Projection(log );

                        await getCommandState.Process(cmdProjection);

                        if ( (cmdProjection.CurrentSequenceNumber > 0) || (cmdProjection.ProjectionValuesChanged()))
                        {
                            if (cmdProjection.CurrentState ==
                                Command_Summary_Projection.CommandState.Completed)
                            {
                                // No need to process a completed projection
#region Logging
                                if (null != log)
                                {
                                    log.LogWarning($"Command {commandId} is complete so no need to process in HandleCreateLeagueCommand");
                                }
#endregion
                                return;
                            }

                            if (cmdProjection.CurrentState ==   Command_Summary_Projection.CommandState.Created )
                            {
                                // No need to process a completed projection
#region Logging
                                if (null != log)
                                {
                                    log.LogWarning($"Command {commandId} is not yet validated so cannot process in HandleCreateLeagueCommand");
                                }
#endregion
                                return;
                            }

                            if (cmdProjection.CurrentState == Command_Summary_Projection.CommandState.Invalid  )
                            {
                                // No need to process a completed projection
#region Logging
                                if (null != log)
                                {
                                    log.LogWarning($"Command {commandId} is not yet marked as invalid so cannot process in HandleCreateLeagueCommand");
                                }
#endregion
                                return;
                            }

                            if (cmdProjection.CurrentState ==
                                Command_Summary_Projection.CommandState.Validated)
                            {

                                string leagueName = string.Empty;
                                
                                // New or previously invalid command can be validated
                                if (cmdProjection.ParameterIsSet(nameof(Create_New_League_Definition.LeagueName)))
                                {
                                    // League name may not be blank
                                    leagueName = cmdProjection.GetParameter<string>( nameof(Create_New_League_Definition.LeagueName));
                                }

                                string location = string.Empty;
                                if (cmdProjection.ParameterIsSet(nameof(Create_New_League_Definition.Location)))
                                {
                                    location  = cmdProjection.GetParameter<string>(nameof(Create_New_League_Definition.Location ));
                                }

                                DateTime dateIncorporated = DateTime.UtcNow ;
                                if (cmdProjection.ParameterIsSet(nameof(Create_New_League_Definition.Date_Incorporated)))
                                {
                                    dateIncorporated = cmdProjection.GetParameter<DateTime>(nameof(Create_New_League_Definition.Date_Incorporated));
                                }

                                // Create a new "League Created" event
                                Leagues.League.eventDefinition.Formed formedEvent = new Leagues.League.eventDefinition.Formed(dateIncorporated ,
                                    location,
                                    $"{leagueName} created by command {commandGuid} ");

                                EventStream leagueEvents = new EventStream(@"Leagues",
                                        "League",
                                        leagueName );

                                if ((null != leagueEvents) && (null != formedEvent))
                                {
                                    await leagueEvents.AppendEvent(formedEvent);
                                }

                                // if there is contact details, add an event for that
                                string emailAddress = string.Empty;
                                if (cmdProjection.ParameterIsSet(nameof(Create_New_League_Definition.Email_Address )))
                                {
                                    emailAddress = cmdProjection.GetParameter<string>(nameof(Create_New_League_Definition.Email_Address));
                                }

                                string twitterHandle = string.Empty;
                                if (cmdProjection.ParameterIsSet(nameof(Create_New_League_Definition.Twitter_Handle )))
                                {
                                    twitterHandle = cmdProjection.GetParameter<string>(nameof(Create_New_League_Definition.Twitter_Handle));
                                }

                                if ((! string.IsNullOrWhiteSpace(emailAddress ) ) || (! string.IsNullOrWhiteSpace(twitterHandle )))
                                {
                                    // Create a new "Contact details changed" event
                                    Leagues.League.eventDefinition.Contact_Details_Changed contactDetailsEvent = new Leagues.League.eventDefinition.Contact_Details_Changed(DateTime.UtcNow,
                                        twitterHandle,
                                        emailAddress);

                                    if ((null != leagueEvents) && (null != contactDetailsEvent))
                                    {
                                        await leagueEvents.AppendEvent(contactDetailsEvent);
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
                                log.LogWarning($"No command events read for {commandId} in ValidateCreateLeagueCommand");
                            }
#endregion
                        }
                    }
                }
            }
        }
    }
}
