using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheLongRun.Common
{
    /// <summary>
    /// A record to store the results returned by a query for cacheing and transmission
    /// </summary>
    public class QueryResultsRecord
    {

        /// <summary>
        /// The folder that new commands are logged to
        /// </summary>
        public const string DEFAULT_CONTAINER_NAME = @"query-results";


        /// <summary>
        /// The unique identifier of this instance of the query
        /// </summary>
        public Guid QueryUniqueIdentifier { get; internal set; }

        /// <summary>
        /// The name of the query that was executed
        /// </summary>
        public string QueryName { get; internal set; }


        /// <summary>
        /// Hash of the query name and parameters used to see if two queries are for the same thing
        /// </summary>
        /// <remarks>
        /// This can be used for results cacheing
        /// </remarks>
        public string Hash { get; }


        /// <summary>
        /// The default filename to use for storing the query results
        /// </summary>
        /// <param name="queryInstance">
        /// The instanve that we are storing the results for
        /// </param>
        public static string MakeResultFilename(QueryResultsRecord queryInstance)
        {
            return queryInstance.QueryName.ToLowerInvariant().Trim() + @"-" + queryInstance.Hash + @".results";
        }
    }

    /// <summary>
    /// Query result record for the given results type
    /// </summary>
    /// <typeparam name="TResults">
    /// The data type of the results returned for the query
    /// </typeparam>
    public class QueryResultsRecord<TResults>
        : QueryResultsRecord 
    {

        /// <summary>
        /// The actual results of the query
        /// </summary>
        /// <remarks>
        /// This may be an individual class or an IEnumerable of row classes
        /// </remarks>
        public TResults Results { get; internal set; }

    }
}
