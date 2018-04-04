using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheLongRun.Common.Attributes
{
    /// <summary>
    /// The query name connected to a given handler function
    /// </summary>
    /// <remarks>
    /// This can be used to create a filename to log the command under etc.
    /// An application may have one or more domains
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class
        , AllowMultiple = false, Inherited = false)]
    public sealed class QueryNameAttribute
        : Attribute 
    {

        private readonly string _queryName;

        public string Name
        {
            get
            {
                return _queryName;
            }
        }


        public QueryNameAttribute(string name)
        {
            _queryName = name;
        }

    }
}
