using System;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using System.Configuration;
using Microsoft.WindowsAzure.Storage.File;
using System.Threading.Tasks;

namespace TheLongRun.Common
{
    public class CommandLogRecord
    {

        /// <summary>
        /// The folder that new commands are logged to
        /// </summary>
        public const string DEFAULT_CONTAINER_NAME = @"command-log";


        /// <summary>
        /// The connection string name to use to write to it
        /// </summary>
        public const string DEFAULT_CONNECTION = @"CommandStorageConnectionAppSetting";

        /// <summary>
        /// The unique identifier of this instance of the command
        /// </summary>
        public Guid CommandUniqueIdentifier { get; protected set; }

        /// <summary>
        /// The name of the command that was executed
        /// </summary>
        public string CommandName { get; protected set; }


        /// <summary>
        /// The status of the command when last executed
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Who triggered this command
        /// </summary>
        public string Who { get; set; }

        /// <summary>
        /// When was this command triggered
        /// </summary>
        public Nullable<DateTime> When { get; set; }

        public static string MakeFilename(CommandLogRecord commandInstance)
        {
            return commandInstance.CommandName.ToLowerInvariant().Trim() + @"-" + commandInstance.CommandUniqueIdentifier.ToString(@"N") + @".cmd";
        }

        public static string MakeFullPath(
            string ContainerName,
            string commandName,
            CommandLogRecord commandInstance)
        {

            if (string.IsNullOrEmpty(ContainerName ))
            {
                ContainerName = CommandLogRecord.DEFAULT_CONTAINER_NAME;
            }

            return ContainerName + @"/" +
                    commandName + @"/" +
                    CommandLogRecord.MakeFilename(commandInstance);
        }
    }

    /// <summary>
    /// A DTO class for logging a command execution
    /// </summary>
    public class CommandLogRecord<TCommandParameters>
        : CommandLogRecord 
    {




        /// <summary>
        /// The parameters of the command instance - these will be saved as JSON
        /// </summary>
        public TCommandParameters Parameters { get; set; }

        /// <summary>
        /// Save to command to a log file 
        /// </summary>
        /// <remarks>
        /// This is required for audit and debug purposes as no command may be executed 
        /// unless it is logged
        /// </remarks>
        public async Task<bool> SaveToFile()
        {

            bool success = false;
            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

            string commandConnection = System.Configuration.ConfigurationManager.ConnectionStrings[DEFAULT_CONNECTION ].ConnectionString;
            if (! string.IsNullOrWhiteSpace(commandConnection ) )
            {
                CloudStorageAccount commandAccount = CloudStorageAccount.Parse(commandConnection);
                CloudFileClient commandClient = commandAccount.CreateCloudFileClient ();
                if (null != commandClient )
                {
                    CloudFileShare commandFileShare = commandClient.GetShareReference(Constants.Container_Command_Log );
                    if (null != commandFileShare )
                    {
                        await commandFileShare.CreateIfNotExistsAsync();
                        CloudFile commandFile = commandFileShare.GetRootDirectoryReference().GetFileReference(MakeFilename(this));
                        if (null != commandFile )
                        {
                            string commandText = Newtonsoft.Json.JsonConvert.SerializeObject(this);  
                            
                            if (! string.IsNullOrWhiteSpace(commandText))
                            {
                                await commandFile.UploadTextAsync (commandText);
                                success = true;
                            }
                        }
                    }
                }
            }

            return success;
        }


        /// <summary>
        /// Create a new command log record
        /// </summary>
        /// <param name="commandName">
        /// The name of the command we are going to log
        /// </param>
        public static CommandLogRecord<TCommandParameters> Create(string commandName,
            TCommandParameters parameters)
        {
            return  new CommandLogRecord<TCommandParameters>()
            {
                CommandUniqueIdentifier = System.Guid.NewGuid(),
                CommandName = commandName,
                When = DateTime.UtcNow,
                Parameters = parameters
            };
            
        }


    }
}
