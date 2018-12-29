using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheLongRun.Common
{
    public static class Constants
    {

        #region Queues
        /// <summary>
        /// The queue that projection run requests are put on
        /// </summary>
        public const string Queue_Projection_Run = @"request-projection-run";

        /// <summary>
        /// The queue that classifier run requests are put on
        /// </summary>
        public const string Queue_Classifier_Run = @"request-classifier-run";
        #endregion

        #region Containers

        /// <summary>
        /// The container that new commands are logged to
        /// </summary>
        public const string Container_Command_Log = @"command-log";

        /// <summary>
        /// The container that command errors are logged to
        /// </summary>
        public const string Container_Command_Error = @"command-error";

        /// <summary>
        /// The container that new commands that are too big to come in as custom messages go in
        /// </summary>
        public const string Container_In_Tray = @"in-tray";

        /// <summary>
        /// The container that event streams are written into
        /// </summary>
        public const string Container_EventStream = @"eventstream";

        /// <summary>
        /// Where snapshots of projections are to be saved
        /// </summary>
        public const string Container_Projection_Snapshot = @"projection-snapshots";

        /// <summary>
        /// Where snapshots of classifiers are to be saved
        /// </summary>
        public const string Container_Classifier_Snapshot = @"classifier-snapshots";

        /// <summary>
        /// The container that identifier group snapshots are written to
        /// </summary>
        public const string Container_Identifier_Group = @"identifier-groups";

        /// <summary>
        /// The container that new query requests are logged to
        /// </summary>
        public const string Container_Query_Log = @"query-log";

        /// <summary>
        /// The container that new query requests are logged to
        /// </summary>
        public const string Container_Query_Results = @"query-results";

        #endregion

        #region Common Domains
        /// <summary>
        /// Where we put command backing event streams etc.
        /// </summary>
        public const string Domain_Command = "Command";

        /// <summary>
        /// Where we put query backing event streams etc.
        /// </summary>
        public const string Domain_Query = "Query";

        #endregion
    }
}
