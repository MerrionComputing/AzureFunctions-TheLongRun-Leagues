using System;
using System.Collections.Generic;
using System.Text;

namespace TheLongRun.Common.Model
{
    /// <summary>
    /// Interface to be implemented by any model component definition which is backed by an Azure
    /// serverless function implementation
    /// (either in the form or an orchestration or an activity)
    /// </summary>
    public interface IDurableFunctionBackedDefinition
    {

        /// <summary>
        /// The name of the durable function that provides the operation of this model component
        /// </summary>
        string DurableFunctionName { get; }

    }
}
