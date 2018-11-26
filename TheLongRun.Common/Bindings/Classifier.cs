using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheLongRun.Common.Attributes;

using CQRSAzure.EventSourcing;

namespace TheLongRun.Common.Bindings
{
    /// <summary>
    /// Wrapper for a CQRSAzure classifier which is used to calculate membership (or not) of an identifier
    /// group
    /// </summary>
    public class Classifier
    {

        private readonly CQRSAzure.IdentifierGroup.IClassifierProcessorUntyped  _classifierProcessor = null;

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

        public Classifier (string domainName,
                            string aggregateTypeName,
                            string aggregateInstanceKey,
                            string classifierTypeName)
           : this(new ClassifierAttribute(domainName ,
               aggregateTypeName,
               aggregateInstanceKey,
               classifierTypeName ) )
        {

        }

        /// <summary>
        /// Create the classifier from the attribute assigned to it in the function 
        /// parameters
        /// </summary>
        /// <param name="attribute">
        /// The attribute that describes the classifier to use
        /// </param>
        public Classifier(ClassifierAttribute attribute )
        {
            _domainName = attribute.DomainName ;
            _aggregateTypeName = attribute.AggregateTypeName ;
            _aggregateInstanceKey = attribute.InstanceKey ;
            _classifierTypeName = attribute.ClassifierTypeName ;

            if (null == _classifierProcessor)
            {
                // TODO : Cater for different backing technologies... currently just AppendBlob
                //  _classifierProcessor = CQRSAzure.IdentifierGroup.Azure.Blob.AzureBlobClassifierUntyped.CreateClassifierProcessor(attribute )
            }
        }
    }
}
