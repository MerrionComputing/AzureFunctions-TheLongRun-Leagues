using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheLongRun.Common.Attributes;

using CQRSAzure.EventSourcing;
using CQRSAzure.IdentifierGroup;

namespace TheLongRun.Common.Bindings
{
    /// <summary>
    /// Wrapper for a CQRSAzure classifier which is used to calculate membership (or not) of an identifier
    /// group
    /// </summary>
    public class Classifier
    {

        private CQRSAzure.IdentifierGroup.IClassifierProcessorUntyped  _classifierProcessor = null;

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
        /// The type of the classifier we are going to run 
        /// </summary>
        private readonly string _classifierTypeName;
        public string ClassifierTypeName
        {
            get
            {
                return _classifierTypeName;
            }
        }

        private readonly string _connectionStringName;

        /// <summary>
        /// Classify whether the aggregate instance is inside or outside the group as determined by the classifier
        /// </summary>
        /// <param name="classifierToProcess">
        /// The class containing the classifier logic to run
        /// </param>
        /// <param name="effectiveDateTime">
        /// The time as-of which to ge the classificiation (or if null, get the latest possible )
        /// </param>
        /// <param name="forceExclude">
        /// If set, consider anything not classified as included to be excluded
        /// </param>
        /// <param name="projection">
        /// If set and the classification depends on the result of a projection use the projection
        /// </param>
        public async Task<IClassifierDataSourceHandler.EvaluationResult> Classify(IClassifierUntyped classifierToProcess = null, 
            DateTime? effectiveDateTime = null, 
            bool forceExclude = false, 
            IProjectionUntyped projection = null)
        {

            _classifierProcessor = CQRSAzure.IdentifierGroup.Azure.Blob.AzureBlobClassifierUntyped.CreateClassifierProcessor(new ClassifierAttribute(_domainName,
               _aggregateTypeName,
               _aggregateInstanceKey,
               _classifierTypeName), classifierToProcess);

            if (null != _classifierProcessor )
            {
                return await _classifierProcessor.Classify(classifierToProcess,
                    effectiveDateTime,
                    forceExclude,
                    projection); 
            }
            else
            {
                if (forceExclude )
                {
                    return IClassifierDataSourceHandler.EvaluationResult.Exclude;
                }
                else
                {
                    return IClassifierDataSourceHandler.EvaluationResult.Unchanged;
                }
            }

        }


        public Classifier (string domainName,
                            string aggregateTypeName,
                            string aggregateInstanceKey,
                            string classifierTypeName,
                            string connectionStringName = @"")
           : this(new ClassifierAttribute(domainName ,
               aggregateTypeName,
               aggregateInstanceKey,
               classifierTypeName ),
               connectionStringName )
        {

        }

        /// <summary>
        /// Create the classifier from the attribute assigned to it in the function 
        /// parameters
        /// </summary>
        /// <param name="attribute">
        /// The attribute that describes the classifier to use
        /// </param>
        public Classifier(ClassifierAttribute attribute,
            string connectionStringName = @"")
        {
            _domainName = attribute.DomainName ;
            _aggregateTypeName = attribute.AggregateTypeName ;
            _aggregateInstanceKey = attribute.InstanceKey ;
            _classifierTypeName = attribute.ClassifierTypeName ;
            
            if (string.IsNullOrWhiteSpace(connectionStringName ) )
            {
                _connectionStringName = ConnectionStringNameAttribute.DefaultConnectionStringName(attribute);  
            }
            else
            {
                _connectionStringName = connectionStringName;
            }
        }
    }
}
