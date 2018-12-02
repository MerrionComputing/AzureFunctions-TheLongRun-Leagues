using System;
using System.Runtime.Serialization;
using CQRSAzure.EventSourcing;
namespace TheLongRun.Common.Events.Query
{
    /// <summary>
    /// A parameter value to be used when executing the query has been set
    /// </summary>
    [Serializable()]
    [CQRSAzure.EventSourcing.DomainNameAttribute("Query")]
    [CQRSAzure.EventSourcing.Category("Query")]
    public class QueryParameterValueSet
        : IEvent 
    {

        /// <summary>
        /// The name of the parameter
        /// </summary>
 
        public string Name { get; set; }




        public object Value { get; set; }

        public QueryParameterValueSet(string nameIn,
            object valueIn)
        {
            Name = nameIn;
            Value = valueIn;
        }

        /// <summary>
        /// Empty constructor for serialisation
        /// </summary>
        public QueryParameterValueSet()
        {
        }

        public QueryParameterValueSet(SerializationInfo info, StreamingContext context)
        {
            Name = info.GetString(nameof(Name));
            Value = info.GetValue(nameof(Value), typeof(object));
        }

        /// <summary>
        /// Populates a SerializationInfo with the data needed to serialize this event instance
        /// </summary>
        /// <remarks>
        /// The version number is also to be saved
        /// </remarks>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Name), Name);
            info.AddValue(nameof(Value), Value);
        }
    }
}
