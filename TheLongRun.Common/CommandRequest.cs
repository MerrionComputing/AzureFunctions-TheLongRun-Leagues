using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace TheLongRun.Common
{
    /// <summary>
    /// A warpped command with the given parameter payload type
    /// </summary>
    /// <typeparam name="TCommandParameters">
    /// The type that defined the parameters to pass to the command
    /// </typeparam>
    public class CommandRequest<TCommandParameters>
    {

        /// <summary>
        /// Text description of the current status of the command request
        /// </summary>
        /// <remarks>
        /// This can allow a command to be retried if neccessary
        /// </remarks>
        public string Status { get; set; }

        /// <summary>
        /// The system wide unique identifier by which this instance of a command request is known
        /// </summary>
        /// <remarks>
        /// If this is not specified, the command handler is responsible for creating an unique GUID to use
        /// </remarks>
        public Guid CommandUniqueIdentifier { get; set; }

        /// <summary>
        /// The underlying parameters for the command
        /// </summary>
        public JObject Parameters { get; set; }

        /// <summary>
        /// The name by which this type of command is known
        /// </summary>
        public string CommandName { get; set; }

        public TCommandParameters GetParameters()
        {
            if (null != Parameters)
            {
                return Parameters.ToObject<TCommandParameters>();
            }
            else
            {
                return default(TCommandParameters);
            }
        }

        public void SetParameters(TCommandParameters parameters)
        {
            if (null != parameters)
            {
                Parameters = JObject.FromObject(parameters);
            }
            else
            {
                Parameters = null;
            }
        }
    }
}
