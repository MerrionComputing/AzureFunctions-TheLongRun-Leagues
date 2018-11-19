using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TheLongRun.Common
{

    public class QueryRequest<TQueryParameters>
    {
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
        public TQueryParameters Parameters { get;  set; }

    }
}
