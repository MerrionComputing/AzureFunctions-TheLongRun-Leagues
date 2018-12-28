using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CQRSAzure.EventSourcing;
using CQRSAzure.IdentifierGroup;
using CQRSAzure.IdentifierGroup.Azure.Blob.Untyped;
using TheLongRun.Common.Attributes;

namespace TheLongRun.Common.Bindings
{
    public class IdentifierGroup
    {

        private  IIdentifierGroupProcessorUntyped _processor;

        /// <summary>
        /// The domain name the aggregate instances of the identifier group belongs to
        /// </summary>
        private readonly string _domainName;
        public string DomainName
        {
            get
            {
                return _domainName;
            }
        }

        /// <summary>
        /// The aggregate type to which the event streams of the identifier group belong
        /// </summary>
        private readonly string _aggregateTypeName;
        public string AggregateTypeName
        {
            get
            {
                return _aggregateTypeName;
            }
        }


        /// <summary>
        /// The name of the identifier group to get the members of
        /// </summary>
        private readonly string _identifierGroupName;
        public string IdentifierGroupName
        {
            get
            {
                return _identifierGroupName;
            }
        }

        /// <summary>
        /// The name of the connection string used to find the event streams that represent 
        /// members of the group
        /// </summary>
        private readonly string _connectionStringName;
        public string ConnectionStringName
        {
            get
            {
                return _connectionStringName;
            }
        }

        public async Task<IEnumerable<string>> GetAll(DateTime? effectiveDateTime  = null)
        {

            _processor = new AzureBlobIdentifierGroupProcessorUntyped(_domainName,
                _aggregateTypeName,
                _connectionStringName ,
                ConnectionStringNameAttribute.DefaultBlobStreamSettings(_domainName, _aggregateTypeName));

            if (null == _processor )
            {
                // Cannot get the members anyway
                return Enumerable.Empty<string>(); 
            }
            else
            {
                return await _processor.GetAll(effectiveDateTime);
            }
        }


        public IdentifierGroup(string domainName,
            string aggregateTypeName,
            string groupName,
            string connectionStringName = @"")
            : this(new IdentifierGroupAttribute(domainName, aggregateTypeName, groupName),
                  connectionStringName )
        {

        }

        public IdentifierGroup(IdentifierGroupAttribute identifierGroup ,
            string connectionStringName = @"")
        {
            _domainName = identifierGroup.DomainName;
            _aggregateTypeName = identifierGroup.AggregateTypeName;
            _identifierGroupName = identifierGroup.IdentifierGroupName;

            if (string.IsNullOrWhiteSpace(connectionStringName))
            {
                _connectionStringName = ConnectionStringNameAttribute.DefaultConnectionStringName(identifierGroup.DomainName , identifierGroup.AggregateTypeName);
            }
            else
            {
                _connectionStringName = connectionStringName;
            }
        }
    }
}
