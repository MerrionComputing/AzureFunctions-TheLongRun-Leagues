using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Azure.WebJobs.Host.Protocols;
using Microsoft.Azure.WebJobs.Host.Triggers;
using Newtonsoft.Json.Linq;


namespace TheLongRun.Common.Orchestration.Bindings
{
    /// <summary>
    /// A trigger binding provider for the "Classifier" typer of event stream 
    /// backed orchestrator
    /// </summary>
    internal class EventStreamBackedClassifierOrchestratorTriggerBindingProvider
        : ITriggerBindingProvider
    {

    }


    /// <summary>
    /// The trigger binding for an event stream backed orchestration for a "Classifier"
    /// </summary>
    private class EventStreamBackedClassifierOrchestrationTriggerBinding 
        : ITriggerBinding
    {


        private readonly ParameterInfo parameterInfo;
        private readonly string classifierOrchestratorName;


        public IReadOnlyDictionary<string, Type> BindingDataContract { get; }

        /// <summary>
        /// The trigger for this binding is an EventStreamBackedClassifierOrchestrator
        /// </summary>
        public Type TriggerValueType => typeof(EventStreamBackedClassifierOrchestrator);

        public EventStreamBackedClassifierOrchestrationTriggerBinding(
            ParameterInfo parameterInfo,
            string orchestratorName)
        {

            this.parameterInfo = parameterInfo;
            this.classifierOrchestratorName = orchestratorName;

            this.BindingDataContract = GetBindingDataContract(parameterInfo);

        }



        public Task<ITriggerData> BindAsync(object value, ValueBindingContext context)
        {
            var orchestrationContext = (EventStreamBackedClassifierOrchestrator)value;
            Type destinationType = this.parameterInfo.ParameterType;



            object convertedValue = null;

            if (destinationType == typeof(EventStreamBackedClassifierOrchestrator))
            {
                convertedValue = orchestrationContext;
            }
            else if (destinationType == typeof(string))
            {
                convertedValue = EventStreamBackedClassifierOrchestrationToString(orchestrationContext);
            }

            ObjectValueProvider inputValueProvider = new ObjectValueProvider(
                convertedValue ?? value,
                this.parameterInfo.ParameterType);


            var bindingData = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            bindingData[this.parameterInfo.Name] = convertedValue;

            // We don't specify any return value binding 
            var triggerData = new TriggerData(inputValueProvider, bindingData);
            return Task.FromResult<ITriggerData>(triggerData);
        }

        /// <summary>
        /// Returns a ParameterDescriptor describing the parameter.
        /// </summary>
        /// <remarks>
        /// Tthe ParameterDescriptor is the mechanism used to integrate with the WebJobs SDK 
        /// Dashboard - it can return ParameterDisplayHints which dictate how the parameter 
        /// is shown in the Dashboard.
        /// </remarks>
        public ParameterDescriptor ToParameterDescriptor()
        {

            ParameterDisplayHints displayHints = new ParameterDisplayHints()
            {
                Description = "Trigger for an event stream backed classifier orchestration",
                Prompt = "Trigger for the specified classifier"
            };

            return new ParameterDescriptor { Name = this.parameterInfo.Name ,
                Type = nameof(EventStreamBackedClassifierOrchestrator),
                DisplayHints =displayHints  };
        }


        /// <summary>
        /// Turn the EventStreamBackedClassifierOrchestrator to a JSON string
        /// </summary>
        /// <param name="orchestrationContext"></param>
        /// <returns></returns>
        private static string EventStreamBackedClassifierOrchestrationToString(EventStreamBackedClassifierOrchestrator orchestrationContext)
        {
            var contextObject = new JObject(
                new JProperty("input", orchestrationContext));
   
            return contextObject.ToString();
        }


    }
}
