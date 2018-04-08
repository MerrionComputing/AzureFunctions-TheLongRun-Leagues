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
        private readonly string _name;
        public string Name => _name;

        

        private readonly object _value;
        public object Value => _value;

        public ParameterValueSet(string nameIn,
            object valueIn)
        {
            _name = nameIn;
            _value = valueIn;
        }


        public ParameterValueSet(SerializationInfo info, StreamingContext context)
        {
            _name = info.GetString(nameof(Name ));
            _value  = info.GetValue(nameof(Value ) , typeof(object));
        }

        /// <summary>
        /// Populates a SerializationInfo with the data needed to serialize this event instance
        /// </summary>
        /// <remarks>
        /// The version number is also to be saved
        /// </remarks>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Name ), _name );
            info.AddValue(nameof(Value ), _value );
        }
    }
}
