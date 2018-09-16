using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace TheLongRun.Common.Orchestration.Bindings
{
    /// <summary>
    /// Hewlper function for data conversions relating to bindings
    /// </summary>
    internal class BindingHelper
    {


#if TODO
        // Work out how bind 
        public StartCommandOrchestrationArgs JObjectToStartCommandtOrchestrationArgs(JObject input, 
            OrchestrationClientAttribute attr)
        {
            return input?.ToObject<StartCommandOrchestrationArgs>();
        }

        public StartCommandOrchestrationArgs StringToStartCommandOrchestrationArgs(string input, 
            OrchestrationClientAttribute attr)
        {
            return !string.IsNullOrEmpty(input) ? JsonConvert.DeserializeObject<StartCommandOrchestrationArgs>(input) : null;
        }

#endif 
    }
}
