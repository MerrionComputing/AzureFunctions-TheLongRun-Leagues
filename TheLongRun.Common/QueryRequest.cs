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
        /// <remarks>
        /// This allows for the query to be resent as a retry
        /// </remarks>
        public string Status { get; set; }


        public QueryResponseTarget[] ResponseTargets { get; set; }

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
