using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheLongRun.Common.Attributes;

using CQRSAzure.EventSourcing;
using TheLongRun.Common.Orchestration;

namespace TheLongRun.Common.Bindings
{
    /// <summary>
    /// A class that takes care of running projections over the underlying event stream
    /// </summary>
    public class Projection
    {

        private readonly IProjectionProcessorUntyped _projectionProcessor = null;

        /// <summary>
        /// The domain name the aggregate instance belongs to
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
        /// The aggregate type to which the event stream belongs
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
        /// The unique identifier of the specific instance of the aggregate
        /// </summary>
        private readonly string _aggregateInstanceKey;
        public string AggregateInstanceKey
        {
            get
            {
                return _aggregateInstanceKey;
            }
        }

        /// <summary>
        /// The type of the projection we are going to run 
        /// </summary>
        private readonly string _projectionTypeName;
        public string ProjectionTypeName
        {
            get
            {
                return _projectionTypeName;
            }
        }

        public async Task  Process(IProjectionUntyped projectionToProcess)
        {
            if (null != _projectionProcessor )
            {
               await _projectionProcessor.Process(projectionToProcess);
            }
        }

        /// <summary>
        /// Process the projection asynchronously and return the end state of the projection
        /// </summary>
        /// <param name="projectionToProcess">
        /// The projection to run
        /// </param>
        /// <returns>
        /// The sequence number up until which the projection was run
        /// </returns>
        public async Task<IProjectionResponse> ProcessAsync(IProjectionUntyped projectionToProcess,
            OrchestrationCallbackIdentity responseSource = null)
        {
            await  Process(projectionToProcess);
            return ProjectionResponse.Create(projectionToProcess, responseSource ); 
        }

        private readonly string _connectionStringName;
        public string ConnectionStringName
        {
            get
            {
                return _connectionStringName;
            }
        }

        public Projection (string domainName,
                            string aggregateTypeName,
                            string aggregateInstanceKey,
                            string projectionTypeName)
            : this(new ProjectionAttribute(domainName ,
                aggregateTypeName,
                aggregateInstanceKey ,
                projectionTypeName ) )
        {
        }

        /// <summary>
        /// Create the projection from the attribute linked to the function parameter
        /// </summary>
        /// <param name="attribute">
        /// The attribute describing which projection to run
        /// </param>
        public Projection(ProjectionAttribute attribute,
            string connectionStringName = "")
        {

            _domainName = attribute.DomainName ;
            _aggregateTypeName = attribute.AggregateTypeName;
            _aggregateInstanceKey = attribute.InstanceKey;
            _projectionTypeName = attribute.ProjectionTypeName;


            if (string.IsNullOrWhiteSpace(connectionStringName))
            {
                _connectionStringName = ConnectionStringNameAttribute.DefaultConnectionStringName(attribute);
            }
            else
            {
                _connectionStringName = connectionStringName;
            }

            if (null == _projectionProcessor)
            {

                // TODO : Allow for different backing technologies... currently just AppendBlob
                _projectionProcessor = CQRSAzure.EventSourcing.Azure.Blob.Untyped.BlobEventStreamReaderUntyped.CreateProjectionProcessor(attribute, 
                    ConnectionStringNameAttribute.DefaultBlobStreamSettings(_domainName,_aggregateTypeName)  );
            }

        }

    }


}
