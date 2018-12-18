using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace TheLongRun.Common.Attributes.Settings
{
    /// <summary>
    /// A mapping between an entity type and the mechanism used to store it in the backing event stream store
    /// </summary>
    /// <remarks>
    /// These can be stored in azure function apps with the name:
    /// TYPEMAP_{precedence}_{domain}_{aggregatetype}
    /// And the value containing the details needed to create the type mapping (; separated):
    /// BlobStream;CommandStorageConnectionString
    /// </remarks>
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

        public const string MAPPING_PREFIX = "TYPEMAP_";
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

        /// <summary>
        /// Turn an azure function app setting to an aggregate type mapping
        /// </summary>
        /// <param name="ApplicationSettingName">
        /// The function app setting name - this must start with TYPEMAP_ 
        /// </param>
        /// <param name="ApplicationSettingValue">
        /// The application setting value to use - semi colon separated
        /// </param>
        /// <remarks>
        /// These can be stored in azure function apps with the name:
        /// TYPEMAP_{precedence}_{domain}_{aggregatetype}
        /// And the value containing the details needed to create the type mapping (; separated):
        /// BlobStream;CommandStorageConnectionString
        /// </remarks>
        public static AggregateTypeMapping MappingFromApplicationSetting(string ApplicationSettingName,
            string ApplicationSettingValue)
        {

            if (string.IsNullOrWhiteSpace(ApplicationSettingName ) )
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(ApplicationSettingValue))
            {
                return null;
            }

            if (!ApplicationSettingName.StartsWith(AggregateTypeMapping.MAPPING_PREFIX ))
            {
                return null;
            }

            AggregateTypeMapping ret = new AggregateTypeMapping();

            string[] nameSections = ApplicationSettingName.Split('_');
            if (nameSections.Length > 1 )
            {
                int precedence = 0;
                if (int.TryParse(nameSections[1], out precedence ))
                {
                    ret.Precedence = precedence;
                }
            }
            if (nameSections.Length > 2 )
            {
                ret.MappedDomainName = nameSections[2];
            }
            else
            {
                ret.MappedDomainName = AggregateTypeMapping.MATCH_ALL; 
            }

            if (nameSections.Length > 3)
            {
                ret.MappedAggregateTypeName = nameSections[3];
            }
            else
            {
                ret.MappedAggregateTypeName = AggregateTypeMapping.MATCH_ALL;
            }

            string[] valueSections = ApplicationSettingValue.Split(';');
            foreach (string valueSection in valueSections )
            {
                if (valueSection.Equals("BlobStream") )
                {
                    ret.StorageType = BackingStorageType.BlobStream;
                }
                if (valueSection.Equals("FileStream"))
                {
                    ret.StorageType = BackingStorageType.FileStream ;
                }
                if (valueSection.Equals("TableStream"))
                {
                    ret.StorageType = BackingStorageType.TableStream ;
                }
            }

            switch (ret.StorageType )
            {
                case BackingStorageType.BlobStream:
                    {
                        ret.BlobStreamSettings = new BlobStreamSettings();
                        // Populate a blob stream settings from the application settings
                        if (valueSections.Length > 1)
                        {
                            ret.BlobStreamSettings.ConnectionStringName = valueSections[1];
                        }
                        if (valueSections.Length > 2)
                        {
                            ret.BlobStreamSettings.ReadSideConnectionStringName = valueSections[2]; 
                        }
                        ret.BlobStreamSettings.DomainName = ret.MappedDomainName;
                        break;
                    }
                case BackingStorageType.FileStream:
                    {
                        ret.FileStreamSettings = new FileStreamSettings();
                        // Populate a file stream settings from the application settings
                        break;
                    }
                case BackingStorageType.TableStream:
                    {
                        ret.TableStreamSettings = new TableStreamSettings();
                        // Populate a table stream settings from the application settings
                        break;
                    }
                default:
                    break;
            }

            return ret;
        }


        private static List<AggregateTypeMapping> _configuredAggregateTypeMappings = null;

        public static List<AggregateTypeMapping> ConfiguredAggregateTypeMappings
        {
            get {
                if (null == _configuredAggregateTypeMappings )
                {
                    var config = new ConfigurationBuilder()
                                    .AddEnvironmentVariables()
                                    .Build();

                    foreach (var item in config.AsEnumerable() )
                    {
                        if (item.Key.StartsWith(AggregateTypeMapping.MAPPING_PREFIX  ) )
                        {
                            _configuredAggregateTypeMappings.Add(AggregateTypeMapping.MappingFromApplicationSetting(item.Key, item.Value)); 
                        }
                    }

                    _configuredAggregateTypeMappings = new List<AggregateTypeMapping>();
                }
                return _configuredAggregateTypeMappings;
            }
        }



    }
}
