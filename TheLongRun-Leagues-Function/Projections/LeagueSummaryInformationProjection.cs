using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace TheLongRunLeaguesFunction.Projections
{
    public static class LeagueSummaryInformationProjection
    {
        /// <summary>
        /// Run the [League Summary Information] projection for the given league and
        /// return the result
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("LeagueSummaryInformationProjection")]
        public static async Task<HttpResponseMessage> LeagueSummaryInformationProjectionRun([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, 
            TraceWriter log)
        {


            const string PROJECTION_NAME = @"league-summary-information";

            // parse query parameter
            string name = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "name", true) == 0)
                .Value;

            // Get request body
            dynamic data = await req.Content.ReadAsAsync<object>();

            // Set name to query string or body data
            name = name ?? data?.name;

            if (null == name )
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a name of the league on the query string or in the request body");
            }


            // TODO : Run the projection



            // Log that this step has completed
            if (null != log)
            {
                log.Verbose("Projection run", 
                    source: "LeagueSummaryInformationProjection");
            }


            // Return the results of the projection...
            return  req.CreateResponse(HttpStatusCode.OK, "Hello " + name);
        }
    }
}
