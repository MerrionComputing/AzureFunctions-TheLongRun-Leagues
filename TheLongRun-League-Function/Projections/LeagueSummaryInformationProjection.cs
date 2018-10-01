using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

using TheLongRun.Common;
using TheLongRun.Common.Attributes;
using TheLongRun.Common.Bindings;
using TheLongRun.Common.Events.Command.Projections;
using TheLongRun.Common.Orchestration;

namespace TheLongRunLeaguesFunction.Projections
{

    [ApplicationName("The Long Run")]
    [DomainName("Leagues")]
    [AggregateRoot("League")]
    [ProjectionName("League Summary") ]
    public static class LeagueSummaryInformationProjection
    {

        /// <summary>
        /// Run the league summary projection over the given league name 
        /// </summary>
        /// <param name="callBack">
        /// If set - the function to notify that we are complete
        /// </param>
        /// <returns>
        /// 
        /// </returns>
        [ApplicationName("The Long Run")]
        [DomainName("Leagues")]
        [AggregateRoot("League")]
        [ProjectionName("League Summary")]
        public static async Task<IProjectionResponse> GetLeagueSummaryQueryProjection(string leagueName,
            OrchestrationCallbackIdentity callBack = null )
        {

            // run the League_Summary_Information projection..
            Projection leagueEvents = new Projection("Leagues",
                "League",
                leagueName,
                typeof(Leagues.League.projection.League_Summary_Information).Name);

            if (null != leagueEvents)
            {
                Leagues.League.projection.League_Summary_Information prjLeagueInfo = new Leagues.League.projection.League_Summary_Information();
                IProjectionResponse projectionResponse;
                projectionResponse = await leagueEvents.ProcessAsync(prjLeagueInfo);
                if (null != projectionResponse)
                {
                    // Make a projection response...
                    return projectionResponse;
                }
            }


            throw new System.NotImplementedException();
        }
    }
}
