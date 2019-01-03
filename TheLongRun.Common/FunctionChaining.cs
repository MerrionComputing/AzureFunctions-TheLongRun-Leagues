using System;
using System.Net.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace TheLongRun.Common
{
    /// <summary>
    /// A class for firing off Azure functions by web hook triggering
    /// </summary>
    /// <remarks>
    /// These can be used to allow one azure function to call one (or many) others
    /// </remarks>
    
    public class FunctionChaining
    {


        public const string  APPLICATION_DOMAIN_LEAGUES = @"https://thelongrun-leagues-function.azurewebsites.net/api/";
        public const string APPLICATION_DOMAIN_RACES = @"https://thelongrun-races-function.azurewebsites.net/api/";
        public const string APPLICATION_DOMAIN_RUNNERS = @"https://thelongrun-runners-function.azurewebsites.net/api/";

        #region Private members
        private ILogger log;
        private static readonly HttpClient httpClient;
        #endregion

        #region Function Keys
        // Put these in a key vault and call it instead
        private System.Collections.Generic.Dictionary<string, string> functionKeys = new System.Collections.Generic.Dictionary<string, string>();
        private void InitialiseFunctionKeys()
        {
            // Application "The Long Run" Domain: "Leagues"
            functionKeys.Add(@"CreateLeagueCommandHandler", @"h0G2yKgCPUEyn8KQ1PguLr3bq4l0S0MSdEhqYzG0u//3JpmE01PCkA==");
            functionKeys.Add(@"CreateLeagueCommandValidation", @"sVmj7ATP8BrwOpnaeIxHCT6InQNqCnvForBA2LMpb54WrOPHXEAzrQ==");
            functionKeys.Add(@"GetLeagueSummaryOutputResults", @"7bYvaNYTqBawLoOcejfV7oHdPaiyCmN3FCR1BonsGa17DxCClm29gA==");
            functionKeys.Add(@"GetLeagueSummaryQueryProjectionProcess", @"8HwcWX82LPnh9m79XbjZ4oDcLRUsIASaduEZylOogfapSyoZXOOpXg==");
            functionKeys.Add(@"GetLeagueSummaryQueryProjectionsRequest", @"E5iEPUEXHsIT8NxlDNS5Msz2GcaAa7Py/EWbiLtFuaSljR8t9Ms7xA==");
            functionKeys.Add(@"GetLeagueSummaryQueryValidation", @"aMbIotn5dSDuaOGBINNdpsk51hTRXn4rCZtSYwK8kc4gzIIMHUR8NA==");
            functionKeys.Add(@"SetLeagueEmailAddressCommandHandler", @"ieeNm5IkoK3yUScYyoHRiCakMw/fQfoD/S4zmLHwqdEyGAMlpxXrOA==");
            functionKeys.Add(@"SetLeagueEmailAddressCommandValidation", @"86yrJaKdwoNgZIQ2uGAVPLrG9RQewBdVb/TfHeg5zkqpIF6Re4JSPg==");
        }
        #endregion


        [Obsolete("Use Azure durable functions instead")]
        public async void TriggerCommandByHTTPS(string DomainName,
            string commandName,
            System.Collections.Generic.IEnumerable<Tuple<string ,string > > parameters,
            HttpContent messageContent = null)
        {
            
            string uriPath = string.Empty ;
            //System.Text.StringBuilder uriBuilder = new System.Text.StringBuilder();
            if (DomainName.ToUpperInvariant() == @"LEAGUES")
            {
                uriPath = APPLICATION_DOMAIN_LEAGUES + commandName;
            }
            if (DomainName.ToUpperInvariant() == @"RACES")
            {
                uriPath = APPLICATION_DOMAIN_RACES + commandName;
            }
            if (DomainName.ToUpperInvariant() == @"RUNNERS")
            {
                uriPath = APPLICATION_DOMAIN_RUNNERS + commandName;
            }

            if (!string.IsNullOrWhiteSpace(uriPath))
            {
                UriBuilder cmdUri = new UriBuilder(uriPath);

                string queryPart = "code=" + functionKeys[commandName];
                foreach (var paramPair in parameters)
                {
                    queryPart += @"&" + paramPair.Item1 + @"=" + paramPair.Item2;
                }
                if (!string.IsNullOrWhiteSpace(cmdUri.Query))
                {
                    cmdUri.Query = cmdUri.Query.Substring(1) + queryPart;
                }
                else
                {
                    cmdUri.Query = queryPart;
                }



                #region Logging
                if (null != log)
                {
                    log.LogInformation($"Sending command to {DomainName }.{commandName } in FunctionChaining");
                    log.LogDebug(cmdUri.Uri.ToString());
                }
                #endregion

                HttpResponseMessage response = await httpClient.PostAsync(cmdUri.Uri, messageContent);

                #region Logging
                if (null != log)
                {
                    log.LogDebug($"Command response - {response}");
                }
                #endregion
            }
        }


        public async void SendQueryResultsByWebhook(string hookTarget,
            string resultValue)
        {

            StringContent messageContent = new StringContent(resultValue);

            UriBuilder cmdUri = new UriBuilder(hookTarget );

            #region Logging
            if (null != log)
            {
                log.LogInformation($"Sending query results to {hookTarget} in FunctionChaining");
                log.LogDebug(cmdUri.Uri.ToString());
            }
            #endregion

            HttpResponseMessage response = await httpClient.PostAsync(cmdUri.Uri, messageContent);

            #region Logging
            if (null != log)
            {
                log.LogDebug($"Query webhook response - {response}");
            }
            #endregion
        }

        public FunctionChaining(ILogger logInput = null)
        {
            if (null != logInput )
            {
                log = logInput;
            }
            InitialiseFunctionKeys();
        }

        static FunctionChaining()
        {
            httpClient = new HttpClient();
        }

    }
}
