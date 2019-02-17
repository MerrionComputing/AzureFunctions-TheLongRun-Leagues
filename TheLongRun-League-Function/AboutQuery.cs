using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using TheLongRun.Common;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using TheLongRun.Common.Attributes;
using TheLongRun.Common.Bindings;

namespace TheLongRunLeaguesFunction
{
    public static class AboutQuery
    {
        /// <summary>
        /// Query to test the [Leagues] domain function app is up and running
        /// </summary>
        /// <remarks>
        /// This is just for debugging - it is not part of the business domain itself
        /// </remarks>
        [FunctionName("AboutQuery")]
        public static async Task<HttpResponseMessage> AboutQueryRun(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]HttpRequestMessage req,
            ILogger log,
            [EventStream("The Long Run", "Test Case", "123")] EventStream esTest,
            [Projection("The Long Run", "Test Case", "123", "Test Projection")] Projection prjTest,
            [Classifier("The Long Run", "Test Case", "123", "Test Classifier")] Classifier clsTest)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");


            if (null != esTest )
            {
                log.LogInformation($"Event stream created {esTest}");
            }
            else
            {
                log.LogWarning($"Unable to create event stream parameter");
            }

            if (null != prjTest)
            {
                log.LogInformation($"Projection created {prjTest}");
            }
            else
            {
                log.LogWarning($"Unable to create projection parameter");
            }

            if (null != clsTest)
            {
                log.LogInformation($"Classifier created {clsTest}");
            }
            else
            {
                log.LogWarning($"Unable to create classifier parameter");
            }

            // parse query parameter
            string name = req.GetQueryNameValuePairsExt()[@"name"];

            // Get request body
            dynamic data = await req.Content.ReadAsAsync<object>();

            // Set name to query string or body data
            name = name ?? data?.name;

            return name == null
                ? req.CreateResponse(HttpStatusCode.OK, "The Long Run - Leagues Domain - Function App")
                : req.CreateResponse(HttpStatusCode.OK,  GetFunctionAbout(name));
        }

        private static string GetFunctionAbout(string functionName)
        {
            switch (functionName )
            {
                case @"OnCreateLeagueCommand":
                    {
                        return @"OnCreateLeagueCommand - Creates a new league";
                    }
                default:
                    {
                        return @"Unknown function name specified - " + functionName;
                    }
            }
        }

    }
}
