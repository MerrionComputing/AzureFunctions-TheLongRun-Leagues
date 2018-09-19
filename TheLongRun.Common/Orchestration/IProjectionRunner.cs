using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Puppy = System.Object;

namespace TheLongRun.Common.Orchestration
{
    /// <summary>
    /// Interface to be implemented by any orchestration type that can spawn a projection to run
    /// </summary>
    public interface IProjectionRunner
    {

        /// <summary>
        /// Run a projection and return the results to the caller's event stream
        /// </summary>
        /// <param name="projectionName">
        /// The domain qualified name of the projection to run
        /// </param>
        /// <param name="instanceId">
        /// The global identifier of the projection instance if we are re-using an existing one (to get an up-to-date value for instance)
        /// </param>
        /// <param name="aggregateKey">
        /// The unique identifier of the aggregate over which the projection is to be run
        /// </param>
        /// <param name="asOfDate">
        /// The date up until which the projection should run
        /// </param>
        /// <param name="asOfSequence">
        /// The event sequence number up until which the projection should run
        /// </param>
        /// <remarks>
        /// If neither as-of-date nor as-of-sequence are supplied, the projection is run to the latest event
        /// </remarks>
        Task<Puppy> RunProjectionAsync(string projectionName, string instanceId, string aggregateKey, 
            DateTime? asOfDate = null,
            int? asOfSequence = null);

    }
}
