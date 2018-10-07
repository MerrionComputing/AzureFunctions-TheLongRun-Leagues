using CQRSAzure.EventSourcing;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheLongRun.Common.Orchestration
{
    public sealed class ProjectionResponse
        : IProjectionResponse
    {

        private readonly DateTime? _asOfDate;
        public DateTime? AsOfDate
        {
            get
            {
                return _asOfDate;
            }
        }

        private readonly int _asOfSequenceNumber;
        public int AsOfSequenceNumber
        {
            get
            {
                return _asOfSequenceNumber;
            }
        }

        private readonly OrchestrationCallbackIdentity _responseSource;
        public OrchestrationCallbackIdentity ResponseSource
        {
            get
            {
                return _responseSource;
            }
        }

        private readonly JArray _values;
        public JArray Values
        {
            get
            {
                return _values;
            }
        }

        private ProjectionResponse(DateTime? responseAsOfDate,
            int asOfSequence,
            JArray projectionValues,
            OrchestrationCallbackIdentity responseSource = null)
        {
            if (responseAsOfDate.HasValue  )
            {
                _asOfDate = responseAsOfDate;
            }
            else
            {
                _asOfDate = DateTime.UtcNow;
            }
            _asOfSequenceNumber = asOfSequence;
            if (null != projectionValues )
            {
                _values = projectionValues;
            }
            if (null != responseSource )
            {
                _responseSource = responseSource;
            }
        }

        public static ProjectionResponse Create(IProjectionUntyped projectionRun,
            OrchestrationCallbackIdentity responseSource = null)
        {
            return new ProjectionResponse(projectionRun.CurrentAsOfDate,
                (int)projectionRun.CurrentSequenceNumber,
                GetValuesAsArray(projectionRun.CurrentValues ),
                responseSource);
        }


        public static JArray GetValuesAsArray(IEnumerable<ProjectionSnapshotProperty> propertyValues)
        {
            if (null == propertyValues)
            {
                // No values returned for the projection
                return null;
            }
            else
            {
                JArray ret = new JArray();
                // Group by ProjectionSnapshotProperty.RowNumber
                var propertyObjectsQuery = propertyValues.OrderBy(record => record.RowNumber )
                    .ToArray();

                int currentRow = 0;
                JObject currentObject = new JObject();
                // hmm...add the name/value pairs as tokens..
                foreach (var item in propertyObjectsQuery)
                { 
                    if (item.RowNumber > currentRow )
                    {
                        ret.Add(currentObject);
                        // new object
                        currentObject = new JObject();
                        currentRow = item.RowNumber;
                    }
                    else
                    {
                        // new property on existing object
                        currentObject.Add(item.Name, JToken.FromObject(item.ValueAsObject));
                    }
                }
                if (null != currentObject )
                {
                    ret.Add(currentObject);
                }
                

                return ret;
            }
        }
    }

    
}
