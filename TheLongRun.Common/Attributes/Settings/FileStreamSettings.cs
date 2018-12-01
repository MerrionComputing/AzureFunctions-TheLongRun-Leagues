

namespace TheLongRun.Common.Attributes.Settings
{
    public class FileStreamSettings
        : CQRSAzure.EventSourcing.Azure.File.IFileStreamSettings
    {

        /// <summary>
        /// The name of the domain in which this entity/attribute exists
        /// </summary>
        public string DomainName { get; set; }

        public long InitialSize { get ; set; }

        public string ConnectionStringName { get ; set ; }

        public string ReadSideConnectionStringName { get ; set ; }

    }
}
