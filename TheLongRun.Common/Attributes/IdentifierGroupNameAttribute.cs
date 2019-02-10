using Microsoft.Azure.WebJobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheLongRun.Common.Attributes
{
    /// <summary>
    /// The identifier group name connected to a given identifier group implementation function
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class
        , AllowMultiple = false, Inherited = false)]
    public sealed  class IdentifierGroupNameAttribute
        : Attribute 
    {

        private readonly string _identifierGroupName;

        public string Name
        {
            get
            {
                return _identifierGroupName;
            }
        }


        public IdentifierGroupNameAttribute(string name)
        {
            _identifierGroupName = name.Trim();
        }


        /// <summary>
        /// Convert this to the default function name to use for an identifier group
        /// </summary>
        /// <returns>
        /// This is to allow a more easy to read set of function names in the attribute/code
        /// </returns>
        public FunctionNameAttribute GetDefaultFunctionName()
        {
            return new FunctionNameAttribute(MakeIdentifierGroupFunctionName(Name));
        }

        public static string MakeIdentifierGroupFunctionName(string identifierGroupName)
        {
            if (identifierGroupName.EndsWith("-IdentifierGroup"))
            {
                return identifierGroupName;
            }
            else
            {
                return identifierGroupName + @"-IdentifierGroup";
            }
        }
    }
}
