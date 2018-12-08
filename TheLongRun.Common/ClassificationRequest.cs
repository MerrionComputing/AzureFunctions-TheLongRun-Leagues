using System;
using System.Collections.Generic;
using System.Text;

namespace TheLongRun.Common
{

    /// <summary>
    /// The definition of a request to run a particular classification
    /// </summary>
    /// <remarks>
    /// Ideally this should only be called from within the domain that owns the classifier
    /// to reduce the risk of coupling issues
    /// </remarks>
    public class ClassificationRequest
    {

        /// <summary>
        /// The name by which this type of classifier is known
        /// </summary>
        public string ClassifierName { get; set; }

        /// <summary>
        /// The unique identifier of the instance to run the classifier for
        /// </summary>
        public string EntityUniqueIdentifier { get; set; }

        /// <summary>
        /// The date/time to run the classifier up to 
        /// </summary>
        /// <remarks>
        /// If not specified this classifier will be run to the current end of the stream
        /// </remarks>
        public Nullable<DateTime> AsOfDate { get; set; }

    }
}
