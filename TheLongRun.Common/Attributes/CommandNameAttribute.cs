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
    /// <remarks>
    /// This can be used to create a filename to log the command under etc.
    /// An application may have one or more domains
    /// </remarks>
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
            if (_commandName.EndsWith("_Command"))
            {
                return new FunctionNameAttribute(_commandName)
;            }
            else
            {
                return new FunctionNameAttribute(_commandName + @"_Command");
            }
        }
    }
}
