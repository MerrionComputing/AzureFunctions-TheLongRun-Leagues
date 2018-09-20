using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace TheLongRun.Common.Orchestration
{
    /// <summary>
    /// Interface to be implemented by any orchestration type that can spawn a classifier to run
    /// </summary>
    public interface IClassifierRunner
    {


        /// <summary>
        /// Run the classifier and return a response indicating if it is in or out of the classification
        /// </summary>
        /// <param name="classifierName">
        /// The domain qualified name of the classifier to run
        /// </param>
        /// <param name="instanceId">
        /// The global identifier of the projection instance if we are re-using an existing one (to get an up-to-date value for instance)
        /// </param>
        /// <param name="aggregateKey">
        /// The unique identifier of the aggregate over which the classifier is to be run
        /// </param>
        /// <param name="asOfDate">
        /// The date up until which the projection should run
        /// </param>
        /// <param name="asOfSequence">
        /// The event sequence number up until which the classifier should run
        /// </param>
        /// <remarks>
        /// If neither as-of-date nor as-of-sequence are supplied, the classifier is run to the latest event
        /// </remarks>
        Task<IClassifierResponse> RunClassifierAsync(string classifierName, string instanceId, string aggregateKey,
            DateTime? asOfDate = null,
            int? asOfSequence = null);

    }

    /// <summary>
    /// The response we get back from a classifier
    /// </summary>
    public interface IClassifierResponse
    {

    }
}
