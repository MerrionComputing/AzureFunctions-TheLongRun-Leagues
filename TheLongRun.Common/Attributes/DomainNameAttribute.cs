using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheLongRun.Common.Attributes
{

    /// <summary>
    /// The domain that a command or query pertains to 
    /// </summary>
    /// <remarks>
    /// This can be used for routing messages and storage
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class , 
        AllowMultiple = false, Inherited = false)]
    public sealed class DomainNameAttribute
        : Attribute 
    {

        private readonly string _domainName;

        public string Name
        {
            get
            {
                return _domainName;
            }
        }


        public DomainNameAttribute(string name)
        {
            _domainName = name;
        }

    }
}
