using System;
using System.Collections.Generic;
using System.Text;

namespace TheLongRun.Common.Attributes
{
    /// <summary>
    /// The connection string to use for this aggrgeate class
    /// </summary>
    /// <remarks>
    /// If not set a default one is used based on the domain name and aggregate type
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class
        , AllowMultiple = false, Inherited = false)]
    public sealed  class ConnectionStringNameAttribute
        : Attribute 
    {

        private readonly string _connectionStringName;

        public string Name
        {
            get
            {
                return _connectionStringName;
            }
        }


        public ConnectionStringNameAttribute(string name)
        {
            _connectionStringName = name;
        }


        public static string DefaultConnectionStringName(CQRSAzure.EventSourcing.IEventStreamUntypedIdentity entity)
        {
            return DefaultConnectionStringName(entity.DomainName, entity.AggregateTypeName);
        }

        public static string DefaultConnectionStringName(string domainName, string aggregateTypeName )
        {
            return $"{domainName}{aggregateTypeName}StorageConnectionString";
        }
    }
}
