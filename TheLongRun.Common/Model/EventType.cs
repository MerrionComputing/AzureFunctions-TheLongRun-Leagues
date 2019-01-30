using System;
using System.Collections.Generic;
using System.Text;

namespace TheLongRun.Common.Model
{
    /// <summary>
    /// A single domain entity event type definition
    /// </summary>
    public class EventType
    {

        /// <summary>
        /// The unique (within this domain entity) name of the event definition
        /// </summary>
        private readonly string _eventTypeName;
        public  string Name
        {
            get
            {
                return _eventTypeName;
            }
        }


        public EventType(string eventTypeName)
        {
            _eventTypeName = eventTypeName;
        }
    }
}
