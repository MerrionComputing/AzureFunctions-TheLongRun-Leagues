using Microsoft.Azure.WebJobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheLongRun.Common.Attributes
{

    /// <summary>
    /// The command name connected to a given handler function
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class
        , AllowMultiple = false, Inherited = false)]
    public sealed class CommandNameAttribute:
        Attribute 
    {

        private readonly string _commandName;

        public string Name
        {
            get
            {
                return _commandName;
            }
        }


        public CommandNameAttribute(string name)
        {
            _commandName = name.Trim();
        }


        /// <summary>
        /// Convert this to the default function name to use for a command
        /// </summary>
        /// <returns>
        /// This is to allow a more easy to read set of function names in the attribute/code
        /// </returns>
        public FunctionNameAttribute GetDefaultFunctionName()
        {
            return new FunctionNameAttribute(MakeCommandFunctionName(_commandName));
        }

        public static string MakeCommandFunctionName(string commandName)
        {
            if (commandName.EndsWith("-Command"))
            {
                return commandName;
            }
            else
            {
                return commandName + @"-Command";
            }
        }
    }
}
