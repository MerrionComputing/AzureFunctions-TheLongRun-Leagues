using CQRSAzure.EventSourcing;
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

        private   ProjectionResponse(DateTime? responseAsOfDate,
            int asOfSequence,
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
            if (null != responseSource )
            {
                _responseSource = responseSource;
            }
        }

        public static ProjectionResponse Create(IProjectionUntyped projectionRun,
            OrchestrationCallbackIdentity responseSource = null)
        {
            // TODO - Package projectionRun.CurrentValues into a JSon object
             

            return new ProjectionResponse(projectionRun.CurrentAsOfDate,
                (int)projectionRun.CurrentSequenceNumber,
                responseSource);
        }
    }
}
