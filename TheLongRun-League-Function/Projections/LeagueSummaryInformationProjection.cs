

using Leagues.League.queryDefinition;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using TheLongRun.Common;
using TheLongRun.Common.Attributes;
using TheLongRun.Common.Bindings;

namespace TheLongRunLeaguesFunction.Projections
{

    [ApplicationName("The Long Run")]
    [DomainName("Leagues")]
    [AggregateRoot("League")]
    [ProjectionName("League Summary")]
    public static class LeagueSummaryInformationProjection
    {

        [ApplicationName("The Long Run")]
        [DomainName("Leagues")]
        [AggregateRoot("League")]
        [ProjectionName("League Summary Information")]
        [FunctionName("RunLeagueSummaryInformationProjectionActivity")]
        public static async Task<Get_League_Summary_Definition_Return> RunLeagueSummaryInformationProjectionActivity(
                [ActivityTrigger] DurableActivityContext context,
                ILogger log)
        {

            #region Logging
            if (null != log)
            {
                log.LogInformation($"GetLeagueSummaryCreateQueryRequestActivity started - instance {context.InstanceId} ");
            }
            #endregion

            ProjectionRequest projectionRequest = context.GetInput<ProjectionRequest>();

            if (null != projectionRequest)
            {
                return await ProcessLeagueSummaryInformationProjection(projectionRequest.ProjectionName,
                    projectionRequest.EntityUniqueIdentifier,
                    log);
            }
            else
            {
                #region Logging
                if (null != log)
                {
                    log.LogInformation($"Unable to read projection request {context.InstanceId} ");
                }
                #endregion
            }

            return null;
        }


        [ApplicationName("The Long Run")]
        [DomainName("Leagues")]
        [AggregateRoot("League")]
        [ProjectionName("League Summary Information")]
        [FunctionName("RunLeagueSummaryInformationProjection")]
        public static async Task<IActionResult> RunLeagueSummaryInformationProjection(
     [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]HttpRequest req,
     ILogger log)
        {

            #region Logging
            if (null != log)
            {
                log.LogDebug("Function triggered HTTP in RunLeagueSummaryInformationProjectionRun");
            }
            #endregion

            // Get the query identifier
            string leagueName = req.Query["LeagueName"];

            if (string.IsNullOrWhiteSpace(leagueName))
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(requestBody);
                leagueName = leagueName ?? data?.LeagueName;
            }


            Get_League_Summary_Definition_Return ret = null;
            string message = $"Running projection for {leagueName}";

            try
            {
                 ret = await ProcessLeagueSummaryInformationProjection("League_Summary_Information",
                    leagueName,
                    log);
            }
            catch (Exception  ex)
            {
                message = ex.ToString();
            }

            

            if (null != ret)
            {
                message = $"{leagueName} Location: {ret.Location } incorporated {ret.Date_Incorporated} (Twitter handle:{ret.Twitter_Handle }) ";
            }

            if (string.IsNullOrWhiteSpace(leagueName))
            {
                return new BadRequestObjectResult($"Please pass a league name to run the projection over");
            }
            else
            {
                return (ActionResult)new OkObjectResult(new {leagueName, message, ret });
            }
        }


        private static async Task<Get_League_Summary_Definition_Return> ProcessLeagueSummaryInformationProjection(
            string projectionName,
            string leagueName,
            ILogger log)
        {


            try
            {
                Projection leagueEvents = new Projection("Leagues",
                    "League",
                    leagueName,
                    projectionName);

                if (null != leagueEvents)
                {
                    Leagues.League.projection.League_Summary_Information prjLeagueInfo = new Leagues.League.projection.League_Summary_Information();
                    await leagueEvents.Process(prjLeagueInfo);
                    if (null != prjLeagueInfo)
                    {
                        if ((prjLeagueInfo.CurrentSequenceNumber > 0) || (prjLeagueInfo.ProjectionValuesChanged()))
                        {
                            return new Get_League_Summary_Definition_Return(Guid.Empty, leagueName)
                            {
                                Date_Incorporated = prjLeagueInfo.Date_Incorporated,
                                Location = prjLeagueInfo.Location,
                                Twitter_Handle = prjLeagueInfo.Twitter_Handle
                            };
                        }
                    }
                }
                else
                {
                    #region Logging
                    if (null != log)
                    {
                        log.LogError("Unable to create league events projection");
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                #region Logging
                if (null != log)
                {
                    log.LogError($"Unable to perform projection {ex.Message}");
                }
                #endregion
            }

            return null;
        }
    }
}
