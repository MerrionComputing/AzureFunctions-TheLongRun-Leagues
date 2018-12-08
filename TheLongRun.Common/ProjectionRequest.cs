using System;
using System.Collections.Generic;
using System.Text;

namespace TheLongRun.Common
{
    /// <summary>
    /// The definition of a request to run a particular projection
    /// </summary>
    /// <remarks>
    /// Ideally this should only be called from within the domain that owns the projection
    /// to reduce the risk of coupling issues
    /// </remarks>
    public class ProjectionRequest
    {

        /// <summary>
        /// The name by which this type of projection is known
        /// </summary>
        public string ProjectionName { get; set; }

        /// <summary>
        /// The unique identifier of the instance to run the projection for
        /// </summary>
        public string EntityUniqueIdentifier { get; set; }

        /// <summary>
        /// The date/time to run the projection up to 
        /// </summary>
        /// <remarks>
        /// If not specified this projection will be run to the current end of the stream
        /// </remarks>
        public Nullable<DateTime > AsOfDate { get; set; }

    }
}
