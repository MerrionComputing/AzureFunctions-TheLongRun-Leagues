using System.IO;
using Leagues.League.commandDefinition;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

using Newtonsoft.Json;
using TheLongRun.Common;
using TheLongRun.Common.Attributes;
using TheLongRun.Common.Bindings;

namespace TheLongRunLeaguesFunction.Commands.Handlers
{
    public static partial class CommandHandler
    {

        
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
        [FunctionName("CreateLeagueCommandHandler")]
        public static void CreateLeagueCommandHandler(
            [BlobTrigger("command-log/create-league/{name}", 
            Connection = "CommandStorageConnectionAppSetting")]Stream  myBlob, 
            string name,
            TraceWriter log)
        {

            #region Logging
            if (null != log)
            {
                log.Info($"Command handler for [create-league] command called\n Payload file name:{name} \n Size: {myBlob.Length} Bytes", 
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
                        log.Info($"Command read from file : {cmdRecord.CommandName} , Id: {cmdRecord.CommandUniqueIdentifier } issued at {cmdRecord.When } by {cmdRecord.Who } ",
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
                log.Verbose ($"Command handler for [create-league] command completed",
                    source: "CreateLeagueCommandHandler");
            }


        }
        
    }
}
