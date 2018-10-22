using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheLongRun.Common.Orchestration
{

    /// <summary>
    /// A response to indicate that a command has completed
    /// </summary>
    /// <remarks>
    /// No status is passed back as we want to allow for completely
    /// "fire-and-forget" execution of commands if the business needs
    /// </remarks>
    public sealed class CommandResponse
        : ICommandResponse
    {

        /// <summary>
        /// The effective date/time as of which this response is valid
        /// </summary>
        private readonly DateTime? _asOfDate;
        public DateTime? AsOfDate
        {
            get
            {
                return _asOfDate;
            }
        }


        /// <summary>
        /// An identifier of the source of this response - in case the calling code
        /// want to rerun it
        /// </summary>
        private readonly OrchestrationCallbackIdentity _responseSource;
        public OrchestrationCallbackIdentity ResponseSource
        {
            get
            {
                return _responseSource;
            }
        }

        private CommandResponse(DateTime? responseAsOfDate,
            OrchestrationCallbackIdentity responseSource = null)
        {
            if (responseAsOfDate.HasValue)
            {
                _asOfDate = responseAsOfDate;
            }
            else
            {
                _asOfDate = DateTime.UtcNow;
            }
            if (null != responseSource)
            {
                _responseSource = responseSource;
            }
        }

        public CommandResponse Create(DateTime? responseAsOfDate,
            OrchestrationCallbackIdentity responseSource = null)
        {
            return new CommandResponse(responseAsOfDate,
                responseSource);
        }
    }
}
