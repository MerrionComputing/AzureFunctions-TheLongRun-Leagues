using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheLongRun.Common.Attributes
{
    /// <summary>
    /// The global application name connected to a given handler function
    /// </summary>
    /// <remarks>
    /// This can be used to route commands 
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class,
        AllowMultiple = false, Inherited = false)]
    public sealed class ApplicationNameAttribute
        : Attribute 
    {

        private readonly string _applicationName;

        public string Name
        {
            get
            {
                return _applicationName;
            }
        }


        public ApplicationNameAttribute(string name)
        {
            _applicationName = name;
        }

    }
}
