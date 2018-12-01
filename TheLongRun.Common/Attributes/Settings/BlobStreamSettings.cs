using CQRSAzure.EventSourcing;

namespace TheLongRun.Common.Attributes.Settings
{
    public class BlobStreamSettings
        : CQRSAzure.EventSourcing.Azure.Blob.IBlobStreamSettings
    {

        /// <summary>
        /// The name of the domain in which this entity/attribute exists
        /// </summary>
        public string DomainName { get; set; }

        /// <summary>
        /// The read/write connection string to use to interact with the event stream
        /// </summary>
        public string ConnectionStringName { get; set; }

        /// <summary>
        /// A more restricted read-only 
        /// </summary>
        public string ReadSideConnectionStringName { get; set; }

    }
}
