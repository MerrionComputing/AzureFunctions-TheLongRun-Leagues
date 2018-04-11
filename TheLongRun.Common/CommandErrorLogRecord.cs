using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheLongRun.Common.Bindings;

namespace TheLongRun.Common
{
    public class CommandErrorLogRecord
    {

        /// <summary>
        /// The folder that any errors are logged to
        /// </summary>
        public const string ERROR_CONTAINER_NAME = @"command-errors";

        /// <summary>
        /// The unique identifier of this instance of the command
        /// </summary>
        public Guid CommandUniqueIdentifier { get; protected set; }

        /// <summary>
        /// The name of the command that was executed
        /// </summary>
        public string CommandName { get; protected set; }

        /// <summary>
        /// The reason the error was logged
        /// </summary>
        public string ErrorMessage { get; protected set; }

        /// <summary>
        /// Log a validation error to the given command event stream
        /// </summary>
        /// <param name="commandGuid">
        /// The unique identifier of the command to log to
        /// </param>
        /// <param name="CommandName">
        /// The name of the command being validated
        /// </param>
        /// <param name="fatal">
        /// True if this is considered a fatal error
        /// </param>
        /// <param name="errorMessage">
        /// 
        /// </param>
        public static void LogCommandValidationError(Guid commandGuid,
            string CommandName,
            bool fatal,
            string errorMessage)
        {

            EventStream commandEvents = new EventStream(@"Command",
                        CommandName,
                        commandGuid.ToString());
            if (null != commandEvents)
            {
                commandEvents.AppendEvent(new TheLongRun.Common.Events.Command.ValidationErrorOccured(errorMessage,fatal ));
            }
        }

        /// <summary>
        /// Mark a command as being valid
        /// </summary>
        /// <param name="commandGuid">
        /// The unique identifier of the command to mark as valid
        /// </param>
        /// <param name="commandName">
        /// The name of the command to mark as valid
        /// </param>
        public static void LogCommandValidationSuccess(Guid commandGuid ,
            string commandName)
        {

            EventStream commandEvents = new EventStream(@"Command",
                    commandName,
                    commandGuid.ToString());
            if (null != commandEvents)
            {
                commandEvents.AppendEvent(new TheLongRun.Common.Events.Command.ValidationSucceeded(DateTime.UtcNow ));
            }
        }

    }


    public class CommandErrorLogRecord<TCommandParameters>
        : CommandErrorLogRecord
    {


        /// <summary>
        /// The parameters of the command instance - these will be saved as JSON
        /// </summary>
        public TCommandParameters Parameters { get; set; }


        public static CommandErrorLogRecord<TCommandParameters>Create(string errorMessage,
            CommandLogRecord<TCommandParameters> commandLog)
        {
            return  new CommandErrorLogRecord<TCommandParameters>()
            {
                CommandUniqueIdentifier = commandLog.CommandUniqueIdentifier,
                CommandName = commandLog.CommandName ,
                ErrorMessage=errorMessage ,

            };
        }



    }
}
