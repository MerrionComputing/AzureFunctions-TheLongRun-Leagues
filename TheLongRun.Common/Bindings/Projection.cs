using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheLongRun.Common.Attributes;

using CQRSAzure.EventSourcing;
using TheLongRun.Common.Orchestration;
using System.Dynamic;

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


        private static  Dictionary<string, Type> _registeredProjections;

        /// <summary>
        /// Find the class that contains the logic of the named projection and return an instance of it
        /// </summary>
        /// <param name="projectionTypeName">
        /// The name of the projection to find
        /// </param>
        /// <remarks>
        /// If no projection is matched by projection name attribute, assume what has been passed
        /// in is the class name (not ideal)
        /// </remarks>
        public static CQRSAzure.EventSourcing.ProjectionBaseUntyped CreateProjectionInstance(
            string projectionTypeName)
        {

            if (null == _registeredProjections)
            {
                RegisterAllLoadedProjections();
            }

            Type returnType = null;

            if (_registeredProjections.ContainsKey(projectionTypeName) )
            {
                returnType = _registeredProjections[projectionTypeName];
            }
            else
            {
                returnType = Type.GetType(projectionTypeName, false);
            }

            if (null != returnType)
            {
                if (returnType.IsSubclassOf(typeof(CQRSAzure.EventSourcing.ProjectionBaseUntyped)))
                {
                    // if it inherits from CQRSAzure.EventSourcing.ProjectionBaseUntyped
                    return (CQRSAzure.EventSourcing.ProjectionBaseUntyped)Activator.CreateInstance(returnType);
                }
            }

            // if we got here then there is no matching projection class we can make
            return null;

        }

        public  static void RegisterAllLoadedProjections()
        {
            _registeredProjections = new Dictionary<string, Type>();

            foreach (System.Reflection.Assembly  assy in AppDomain.CurrentDomain.GetAssemblies())
            {
                if ( (! assy.FullName.StartsWith("System.")) 
                    && (!assy.FullName.StartsWith("Microsoft."))
                    && (!assy.FullName.StartsWith("CQRSAzure.")))
                {
                    if (!assy.IsDynamic)
                    {
                        foreach (Type exportedType in assy.GetTypes())
                        {
                            if (exportedType.IsSubclassOf(typeof(CQRSAzure.EventSourcing.ProjectionBaseUntyped)))
                            {
                                string projectionName = Attributes.ProjectionNameAttribute.GetProjectionName(exportedType);
                                _registeredProjections.Add(projectionName, exportedType);
                            }
                        }
                    }
                }
            }

        }


        public static IEnumerable<object> GetProjectionResults(IEnumerable<ProjectionSnapshotProperty> CurrentValues)
        {
            if (null != CurrentValues )
            {
                List<object> returnValues = new List<object>();
                foreach (int row in CurrentValues.Select(f=>f.RowNumber ).Distinct() )
                {
                    // one object per row...
                    IDictionary<string, object> rowObj = new ExpandoObject();
                    rowObj.Add("Row_Number", row);
                    foreach (var columnValue in CurrentValues.Where(f => f.RowNumber == row  ))
                    {
                        rowObj.Add(MakeValidPropertyName(columnValue.Name), columnValue.ValueAsObject); 
                    }

                    dynamic dynamicRowObject = rowObj;
                    if (null != rowObj)
                    {
                        returnValues.Add(rowObj);
                    }
                }

                return returnValues.AsEnumerable();
            }
            else
            {
                return Enumerable.Empty<object>() ;
            }
        }

        public  static string MakeValidPropertyName(string nameIn)
        {
            return String.Join(@"_", nameIn.Split(@" -!,;':@£$%^ &*() -+=/\#~".ToCharArray() )).Trim();
        }
    }


}
