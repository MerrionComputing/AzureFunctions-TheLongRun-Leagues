using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace TheLongRun.Common
{

    public class ProjectionResultsRecord
    {
        /// <summary>
        /// The unique identifier of what called the projection to be run
        /// </summary>
        public Guid CorrelationIdentifier { get; set; }

        /// <summary>
        /// The name of the query or command that called this projection
        /// </summary>
        public string ParentRequestName { get; set; }

        /// <summary>
        /// The name by which this type of projection is known
        /// </summary>
        public string ProjectionName { get; set; }

        /// <summary>
        /// The domain in which the projection was run
        /// </summary>
        public string DomainName { get; set; }

        /// <summary>
        /// The aggregate type over which the projection was run
        /// </summary>
        public string AggregateTypeName { get; set; }

        /// <summary>
        /// The unique identifier of the instance to run the projection for
        /// </summary>
        public string EntityUniqueIdentifier { get; set; }

        /// <summary>
        /// The as-of date the results were returned for
        /// </summary>
        public Nullable<DateTime> CurrentAsOfDate { get; set; }

        /// <summary>
        /// The as-of sequence number the results were returned for
        /// </summary>
        public int CurrentSequenceNumber { get; set; }

        /// <summary>
        /// This is set to true if an error occured so the projection result should not be trusted
        /// </summary>
        public bool Error { get; set; }

        /// <summary>
        /// Additional information that can be fed back to the caller for logging or progress display
        /// </summary>
        public string StatusMessage { get; set; }




    }

    public class ProjectionResultsRecord<TRecordType>
        : ProjectionResultsRecord
    {


        /// <summary>
        /// The actual data part of the projection result
        /// </summary>
        public TRecordType  Result { get; set; }
    }
}
