using System;
using System.Runtime.Serialization;
using CQRSAzure.EventSourcing;

namespace TheLongRun.Common.Events.Command
{
    /// <summary>
    /// A parameter to be used when processing the command was set
    /// </summary>
    [Serializable()]
    [CQRSAzure.EventSourcing.DomainNameAttribute("Command")]
    [CQRSAzure.EventSourcing.Category("Command")]
    public class ParameterValueSet
        : IEvent
    {

        /// <summary>
        /// The name of the parameter
        /// </summary>
        public string Name { get; set; }

        

        public object Value { get; set; }

        public ParameterValueSet(string nameIn,
            object valueIn)
        {
            Name  = nameIn;
            Value  = valueIn;
        }


        public ParameterValueSet(SerializationInfo info, StreamingContext context)
        {
            Name = info.GetString(nameof(Name ));
            Value   = info.GetValue(nameof(Value ) , typeof(object));
        }

        /// <summary>
        /// Populates a SerializationInfo with the data needed to serialize this event instance
        /// </summary>
        /// <remarks>
        /// The version number is also to be saved
        /// </remarks>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Name ), Name  );
            info.AddValue(nameof(Value ), Value );
        }
    }
}
