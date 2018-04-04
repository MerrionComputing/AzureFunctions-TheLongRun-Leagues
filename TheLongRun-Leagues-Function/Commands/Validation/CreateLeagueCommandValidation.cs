using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

using Leagues.League.commandDefinition;
using TheLongRun.Common;
using TheLongRun.Common.Attributes;

namespace TheLongRunLeaguesFunction.Commands.Validation
{
    /// <summary>
    /// Validation to be performed on the [Create-New-League] command
    /// </summary>
    /// <remarks>
    /// League name may not be empty
    /// League name may not be duplicate
    /// Date_Incorporated may not be future dated
    /// </remarks>
    public static partial class CommandValidator
    {


    }
}
