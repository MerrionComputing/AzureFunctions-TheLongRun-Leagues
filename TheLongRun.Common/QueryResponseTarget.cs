using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace TheLongRun.Common
{
    /// <summary>
    /// Where the output of a query should be sent
    /// </summary>
    /// <remarks>
    /// We allow the query oroginator to specify this but also for the query handler to add their own targets,
    /// for the purposes of cacheing, notifications, auditing or billing etc.
    /// </remarks>
    public class QueryResponseTarget
    {

        /// <summary>
        /// How to indicate to a caller application that a requested query is complete
        /// </summary>
        /// <remarks>
        /// Other ways of notifying the caller that a query has completed can be added to this
        /// list as and when required
        /// </remarks>
        public enum ReturnTargetType
        {
            /// <summary>
            /// No output type specified - just skip over this return target
            /// </summary>
            NotSet = 0,
            /// <summary>
            /// Store the results of the query in the named azure storage blob, which will either trigger
            /// the next step or can be polled for (not ideal)
            /// </summary>
            AzureBlobStorage = 1,
            /// <summary>
            /// Use the defined webhook to pass the results back for the query
            /// </summary>
            WebHook = 2,
            /// <summary>
            /// Fire off an event grid event with the specified custom topic name to notify that the
            /// query results are available
            /// </summary>
            CustomEventGridTopic = 3,
            /// <summary>
            /// Add a message to the given service bus queue to be dealt with by any subscriber(s)
            /// </summary>
            ServiceBus = 4,
            /// <summary>
            /// Send the answer out by a SignalR message
            /// </summary>
            SignalR = 5,
            /// <summary>
            /// Raise an event to notify a durable function orchestration that has been paused waiting for
            /// this query to complete that it is now good to go
            /// </summary>
            DurableFunctionOrchestration = 6
        }

        /// <summary>
        /// Where to notify the requestor that this query has completed
        /// </summary>
        /// <remarks>
        /// This does not preclude other observers also being notified that the 
        /// query has completed
        /// </remarks>
        [JsonConverter(typeof(StringEnumConverter))]
        public ReturnTargetType ReturnTarget { get; set; }

        /// <summary>
        /// Depending on the return target, this tells the query processor where 
        /// exactly to put the results
        /// </summary>
        /// <remarks>
        /// This could be a storeage URI or a webhook or the custom topic name, for instance
        /// </remarks>
        public string ReturnPath { get; set; }

    }
}
