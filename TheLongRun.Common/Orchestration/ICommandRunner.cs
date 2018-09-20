using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheLongRun.Common.Orchestration
{
    public interface ICommandRunner
    {

        /// <summary>
        /// Runs the specified command
        /// </summary>
        /// <param name="commandName">
        /// The domain qualified name of the command to run
        /// </param>
        /// <param name="instanceId">
        /// The global identifier of the command instance if we are re-using an existing one 
        /// </param>
        Task<IQueryResponse> RunCommandAsync(string commandName,
            string instanceId,
            JObject commandParameters);

    }

    public interface ICommandResponse
    {

    }
}
