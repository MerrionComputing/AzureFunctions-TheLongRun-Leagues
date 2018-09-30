using Microsoft.Azure.WebJobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheLongRun.Common.Attributes
{

    /// <summary>
    /// The projection name connected to a given projection implementation function
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class
        , AllowMultiple = false, Inherited = false)]
    public sealed class ProjectionNameAttribute
        : Attribute 
    {

        private readonly string _projectionName;

        public string Name
        {
            get
            {
                return _projectionName;
            }
        }


        public ProjectionNameAttribute(string name)
        {
            _projectionName = name.Trim();
        }


        /// <summary>
        /// Convert this to the default function name to use for a projection
        /// </summary>
        /// <returns>
        /// This is to allow a more easy to read set of function names in the attribute/code
        /// </returns>
        public FunctionNameAttribute GetDefaultFunctionName()
        {
            if (_projectionName.EndsWith("-Projection"))
            {
                return new FunctionNameAttribute(_projectionName)
;
            }
            else
            {
                return new FunctionNameAttribute(_projectionName + @"-Projection");
            }
        }

    }
}
