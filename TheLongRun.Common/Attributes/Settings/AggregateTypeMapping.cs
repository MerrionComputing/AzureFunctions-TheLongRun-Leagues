using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TheLongRun.Common.Attributes.Settings
{
    /// <summary>
    /// A mapping between an entity type and the mechanism used to store it in the backing event stream store
    /// </summary>
    public class AggregateTypeMapping
    {

        public enum BackingStorageType
        {
            /// <summary>
            /// Event stream is stored in append-blob storage
            /// </summary>
            BlobStream = 0,
            /// <summary>
            /// Event stream is in a file 
            /// </summary>
            FileStream = 1,
            /// <summary>
            /// Event stream is in an Azure table
            /// </summary>
            TableStream = 2
        }

        public const string MATCH_ALL = "*";

        /// <summary>
        /// The  order in which mappings should be tested against any aggregate type 
        /// </summary>
        public int Precedence { get; set; }

        /// <summary>
        /// The domain to which this mapping applies
        /// </summary>
        /// <remarks>
        /// This can be MATCH_ALL 
        /// </remarks>
        public  string MappedDomainName { get; set; }

        /// <summary>
        /// The aggregate type to which this mapping applies
        /// </summary>
        /// <remarks>
        /// This can be MATCH_ALL
        /// </remarks>
        public string MappedAggregateTypeName { get; set; }

        /// <summary>
        /// What is used to store the underlying event stream
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public BackingStorageType StorageType { get; set; }

        /// <summary>
        /// Extra settings to use when persisting this aggregate to an append blob
        /// </summary>
        public BlobStreamSettings BlobStreamSettings { get; set; }

        /// <summary>
        /// Extra settings to use when persisting this aggregate to a file
        /// </summary>
        public FileStreamSettings FileStreamSettings { get; set; }

        /// <summary>
        /// Extra settings to use when persisting this aggregate to a table
        /// </summary>
        public TableStreamSettings TableStreamSettings { get; set; }


        public bool Matches(string domainName, string aggregateTypeName)
        {
            if (domainName.Equals(MappedDomainName ) && aggregateTypeName.Equals(MappedAggregateTypeName )  )
            {
                return true;
            }

            if (domainName.Equals(MappedDomainName) && string.IsNullOrWhiteSpace(aggregateTypeName))
            {
                return true;
            }

            if (domainName.Equals(MappedDomainName) && MappedAggregateTypeName.Equals(MATCH_ALL ))
            {
                return true;
            }

            if (MappedDomainName.Equals(MATCH_ALL) && aggregateTypeName.Equals(MappedAggregateTypeName))
            {
                // Match by aggregate type only
                return true;
            }

            if (MappedDomainName.Equals(MATCH_ALL) && MappedAggregateTypeName.Equals(MATCH_ALL))
            {
                // Match to everything not already matched
                return true;
            }

            return false;
        }
    }
}
