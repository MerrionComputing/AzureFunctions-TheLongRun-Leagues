using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheLongRun.Common.Attributes
{

    /// <summary>
    /// The aggregate root name connected to a given handler function
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class
        , AllowMultiple = false, Inherited = false)]
    public sealed class AggregateRootAttribute :
        Attribute
    {

        private readonly string _aggregateRootName;

        public string Name
        {
            get
            {
                return _aggregateRootName;
            }
        }


        public AggregateRootAttribute(string name)
        {
            _aggregateRootName = name;
        }

    }
}
