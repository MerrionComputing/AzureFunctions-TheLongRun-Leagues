using System;
using System.Collections.Generic;
using System.Text;

namespace TheLongRun.Common.Model
{
    /// <summary>
    /// The event type definitions that pertain to a given domain entity type
    /// </summary>
    public class EventTypes
    {

        private readonly Dictionary<string, EventType> _eventTypes;

        /// <summary>
        /// Indexer to get the event type definition by name from this collection
        /// </summary>
        /// <param name="index">
        /// The name of the entity type
        /// </param>
        public EventType  this[string index]
        {
            get
            {
                if (_eventTypes .ContainsKey(index))
                {
                    return _eventTypes[index];
                }
                return null;
            }
        }

        internal void AddEventType(EventType eventTypeToAdd)
        {
            if (! _eventTypes.ContainsKey(eventTypeToAdd.Name) )
            {
                _eventTypes.Add(eventTypeToAdd.Name, eventTypeToAdd);
            }
        }

        public EventTypes()
        {
            _eventTypes = new Dictionary<string, EventType>();
        }


    }

    /// <summary>
    /// Methods for constructing the domain model using a fluent syntax
    /// </summary>
    public static partial class FluentInterface
    {

        /// <summary>
        /// Add a query definition to this domain model
        /// </summary>
        public static EventTypes Add(this EventTypes eventTypes, EventType  eventToAdd)
        {
            eventTypes.AddEventType(eventToAdd);
            return eventTypes;
        }

    }
}
