using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;

namespace TheLongRun.Common
{

    /// <summary>
    /// The definition of a query request from an external system
    /// </summary>
    /// <typeparam name="TQueryParameters">
    /// The type providing the parameters to pass in to the query
    /// </typeparam>
    public class QueryRequest<TQueryParameters> 
    {

        /// <summary>
        /// Text description of the current status of the query request
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Where to notify the requestor that this query has completed
        /// </summary>
        /// <remarks>
        /// This does not preclude other observers also being notified that the 
        /// query has completed
        /// </remarks>
        [JsonConverter(typeof(StringEnumConverter))]
        public QueryLogRecord.QueryReturnTarget ReturnTarget { get;  set; }


        /// <summary>
        /// Depending on the return target, this tells the query processor where 
        /// exactly to put the results
        /// </summary>
        /// <remarks>
        /// This could be a storeage URI or a webhook or the custom topic name, for instance
        /// </remarks>
        public string ReturnPath { get;  set; }

        /// <summary>
        /// The underlying parameters for the query
        /// </summary>
        public JObject  Parameters { get;  set; }

        /// <summary>
        /// The name by which this type of query is known
        /// </summary>
        public string QueryName { get; set; }

        /// <summary>
        /// The system wide unique identifier by which this instance of a query request is known
        /// </summary>
        public Guid QueryUniqueIdentifier { get; set; }

        public TQueryParameters GetParameters()
        {
            if (null != Parameters )
            {
                return Parameters.ToObject<TQueryParameters>()  ; 
            }
            else
            {
                return default(TQueryParameters);
            }
        }

        public void SetParameters(TQueryParameters parameters)
        {
            if (null != parameters )
            {
                Parameters = JObject.FromObject(parameters);
            }
            else
            {
                Parameters = null;
            }
        }



    }
}
