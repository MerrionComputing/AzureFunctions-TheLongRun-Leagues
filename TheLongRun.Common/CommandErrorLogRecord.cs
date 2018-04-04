using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
