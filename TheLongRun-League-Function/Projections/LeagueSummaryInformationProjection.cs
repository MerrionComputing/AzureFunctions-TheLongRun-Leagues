using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

using TheLongRun.Common;

namespace TheLongRunLeaguesFunction.Projections
{
    public static class LeagueSummaryInformationProjection
    {
        /// <summary>
        /// Run the projection for the given league in the message
        /// return the result
        /// </summary>
        /// <param name="projectionRequestQueueItem">
        /// The projection request to process (in the format [projection-name::key] )
        /// </param>
        /// <param name="log">
        /// </param>
        /// <returns></returns>
        [FunctionName("LeagueProjection")]
        public static async void LeagueProjectionRun(
            [QueueTrigger(Constants.Queue_Projection_Run)] string projectionRequestQueueItem,
            TraceWriter log)
        {


            #region Logging
            if (null != log)
            {
                log.Info($"League Projection Run requested - {projectionRequestQueueItem}",
                    source: "LeagueProjectionRun");
            }
            #endregion

            const string PROJECTION_NAME = @"league-summary-information";

            // Run the projection as requested


        }
    }
}
