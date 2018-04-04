using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheLongRun.Common.Attributes
{
    /// <summary>
    /// The event source topic name connected to a given handler function
    /// </summary>
    /// <remarks>
    /// This can be used to connect the function up to the appropriate even topic
    /// (or this can be done by powershell script or in the azure portal)
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class
        , AllowMultiple = false, Inherited = false)]
    public class EventTopicSourceNameAttribute
        : Attribute 
    {

        private readonly string _eventTopicSourceName;

        public string Name
        {
            get
            {
                return _eventTopicSourceName;
            }
        }


        public EventTopicSourceNameAttribute(string name)
        {
            _eventTopicSourceName = name;
        }

    }
}
