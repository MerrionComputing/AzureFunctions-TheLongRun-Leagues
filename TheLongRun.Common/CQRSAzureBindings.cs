using CQRSAzure.EventSourcing;


namespace TheLongRun.Common
{
    /// <summary>
    /// Hard coded bindings to use for the different event sourcing domains
    /// </summary>
    /// <remarks>
    /// This could be set to use a config file in a production system that we wanted to have
    /// [Dev|QA|UAT|Prod] environments
    /// </remarks>
    public static class CQRSAzureBindings
    {

        /// <summary>
        /// The backing store types that are supported by the CQRS on EventGrid framework
        /// </summary>
        /// <remarks>
        /// This is a subset of the CQRSAzure library methods as some don't fit well with the
        /// EventGrid architecture
        /// </remarks>
        public enum BackingStoreType
        {
            /// <summary>
            /// (Default) Store the event streams in an AppendBlob 
            /// </summary>
            /// <remarks>
            /// ImplementationType="AzureBlob"
            /// ConnectionStringName="LeaguesConnectionString" 
            /// DomainName="Leagues"
            /// </remarks>
            AppendBlob = 0,
            /// <summary>
            /// Store the event streams in Azure table storage
            /// </summary>
            /// <remarks>
            /// ImplementationType="AzureTable"
            /// ConnectionStringName="LeaguesConnectionString" 
            /// SequenceNumberFormat="00000000"
            /// </remarks>
            Table = 1,
            /// <summary>
            /// Store the event streams in an Azure file
            /// </summary>
            /// <remarks>
            /// ImplementationType="AzureFile"
            /// ConnectionStringName="LeaguesConnectionString" 
            /// InitialSize="20000" 
            /// </remarks>
            File = 2
        }

        /// <summary>
        /// Settings that can be set for an append blob backed event stream
        /// </summary>
        /// <remarks>
        /// ImplementationType="AzureBlob" ConnectionStringName="LeaguesConnectionString" DomainName="Leagues"
        /// </remarks>
        public class AppendBlobConnectionSettings
            : ConnectionSettingsBase
        {


            public AppendBlobConnectionSettings(string settingName,
                string settingValue)
                : base(settingName )
            {

            }
        }

        /// <summary>
        /// Settings that can be set for an azure table backed event stream
        /// </summary>
        /// <remarks>
        /// ImplementationType="AzureTable" ConnectionStringName="LeaguesConnectionString" SequenceNumberFormat="00000000"
        /// </remarks>
        public class TableConnectionSettings
            : ConnectionSettingsBase
        {

            public const string DEFAULT_SEQUENCE_FORMAT = @"00000000";

            /// <summary>
            /// The format to use when storing the sequence number as a string
            /// </summary>
            /// <remarks>
            /// This is usually zero-padded to make indexing faster and sequence easier
            /// to read
            /// </remarks>
            private readonly string _sequenceNumberFormat;
            public string SequenceNumberFormat
            {
                get
                {
                    if (string.IsNullOrWhiteSpace(_sequenceNumberFormat))
                    {
                        return DEFAULT_SEQUENCE_FORMAT;
                    }
                    else
                    {
                        return _sequenceNumberFormat;
                    }
                }
            }

            public TableConnectionSettings(string settingName,
                    string settingValue)
                    : base(settingName)
            {

            }
        }


        /// <summary>
        /// Settings that can be set for a file backed event stream
        /// </summary>
        /// <remarks>
        /// ImplementationType="AzureFile" ConnectionStringName="LeaguesConnectionString" InitialSize="20000" 
        /// </remarks>
        public class FileConnectionSettings
        {

            private const int DEFAULT_INITIAL_SIZE = 20000;


        }

        /// <summary>
        /// Properties shared by all the different types of connection settings
        /// </summary>
        public abstract class ConnectionSettingsBase
        {

            private string _settingType;
            /// <summary>
            /// The type of setting this pertains to 
            /// </summary>
            /// <remarks>
            /// EventStream or Snapshot
            /// </remarks>
            public string SettingType
            {
                get { return _settingType; }
            }

            private readonly string _mappedDomainName;
            /// <summary>
            /// The domain name the event stream entity belongs to
            /// </summary>
            public string MappedDomainName
            {
                get { return _mappedDomainName; }
            }

            private readonly string _mappedAggregateName;
            /// <summary>
            /// The aggregate type that the event stream is mapped to
            /// </summary>
            public string MappedAggregateName
            {
                get
                {
                    return _mappedAggregateName;
                }
            }

            /// <summary>
            /// Create the settings base from the config property name(dot separated)
            /// </summary>
            /// <param name="settingName">
            /// The unique name of the setting e.g.
            /// EventSteam.Leagues.League
            /// Snapshot.Leagues.League.League_Summary_Information etc.
            /// </param>
            public ConnectionSettingsBase(string settingName)
            {
                if (!string.IsNullOrWhiteSpace(settingName))
                {
                    string[] sections = settingName.Split('.');
                    if (sections.Length> 0 )
                    {
                        _settingType = sections[0];
                    }
                    if (sections.Length > 1)
                    {
                        _mappedDomainName = sections[1];
                    }
                    if (sections.Length > 2)
                    {
                        _mappedAggregateName = sections[2];
                    }
                }
            }

        }
    }
}
