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



namespace TheLongRunLeaguesFunction.Projections
{

    [ApplicationName("The Long Run")]
    [DomainName("Leagues")]
    [AggregateRoot("League")]
    [ProjectionName("League Summary") ]
    public static class LeagueSummaryInformationProjection
    {

    }
}
