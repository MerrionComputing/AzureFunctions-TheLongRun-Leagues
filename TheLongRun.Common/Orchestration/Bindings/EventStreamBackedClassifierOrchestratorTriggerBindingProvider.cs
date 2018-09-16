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
        public Task<ITriggerBinding> TryCreateAsync(TriggerBindingProviderContext context)
        {
            throw new NotImplementedException();
        }



    }


    /// <summary>
    /// The trigger binding for an event stream backed orchestration for a "Classifier"
    /// </summary>
    public class EventStreamBackedClassifierOrchestrationTriggerBinding 
        : ITriggerBinding
    {


        private readonly ParameterInfo parameterInfo;
        private readonly string classifierOrchestratorName;


        public IReadOnlyDictionary<string, Type> BindingDataContract { get; }

        /// <summary>
        /// The trigger for this binding is an EventStreamBackedClassifierOrchestrator
        /// </summary>
        public Type TriggerValueType => typeof(EventStreamBackedClassifierTriggerValue);


        /// <summary>
        /// Constructor to create this binding
        /// </summary>
        /// <param name="parameterInfo">
        /// The parameter the trigger is bound to
        /// </param>
        /// <param name="orchestratorName">
        /// The name of the orchestrator
        /// </param>
        public EventStreamBackedClassifierOrchestrationTriggerBinding(
            ParameterInfo parameterInfo,
            string orchestratorName)
        {

            this.parameterInfo = parameterInfo;
            this.classifierOrchestratorName = orchestratorName;

            this.BindingDataContract = GetBindingDataContract(parameterInfo);

        }

        private IReadOnlyDictionary<string, Type> GetBindingDataContract(ParameterInfo parameterInfo)
        {
            Dictionary<string, Type> contract = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
            contract.Add("EventStreamBackedClassifierTrigger", typeof(EventStreamBackedClassifierTriggerValue));
            return contract;
        }

        public Task<ITriggerData> BindAsync(object value, ValueBindingContext context)
        {
            var orchestrationContext = (EventStreamBackedClassifierTriggerValue)value;
            Type destinationType = this.parameterInfo.ParameterType;



            object convertedValue = null;

            if (destinationType == typeof(EventStreamBackedClassifierTriggerValue))
            {
                convertedValue = orchestrationContext;
            }
            else if (destinationType == typeof(string))
            {
                convertedValue = EventStreamBackedClassifierOrchestrationToString(orchestrationContext);
            }

            return Task.FromException<ITriggerData>(new NotImplementedException());

#if TODO

            // Need to work out how to return the binding ??

            var bindingData = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            bindingData[this.parameterInfo.Name] = convertedValue;

            // We don't specify any return value binding 
            var triggerData = new TriggerData(inputValueProvider, bindingData);
            return Task.FromResult<ITriggerData>(triggerData);
#endif
        }

        /// <summary>
        /// Returns a ParameterDescriptor describing the parameter.
        /// </summary>
        /// <remarks>
        /// The ParameterDescriptor is the mechanism used to integrate with the WebJobs SDK 
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

            return new EventStreamBackedClassifierParameterDescriptor
            { Name = this.parameterInfo.Name ,
                Type = nameof(EventStreamBackedClassifierOrchestrator),
                DisplayHints =displayHints  };
        }


        /// <summary>
        /// Turn the EventStreamBackedClassifierOrchestrator to a JSON string
        /// </summary>
        /// <param name="orchestrationContext"></param>
        /// <returns></returns>
        private static string EventStreamBackedClassifierOrchestrationToString(EventStreamBackedClassifierTriggerValue orchestrationContext)
        {
            var contextObject = new JObject(
                new JProperty("input", orchestrationContext));
   
            return contextObject.ToString();
        }

        /// <summary>
        /// Creates a listener to trigger the classifier orchestration
        /// </summary>
        /// <param name="context"></param>
        /// <remarks>
        /// The runtime will call this method to create the listener for the trigger event source. 
        /// A ListenerFactoryContext object is passed to the method. 
        /// It contains a ITriggeredFunctionExecutor instance, which is what the listener uses to call 
        /// back into the runtime when an event is triggered. When the listener fires it calls back into 
        /// the runtime with a trigger value. The runtime will then flow that value back through BindAsync 
        /// during the bind process when the function is invoked.
        /// </remarks>
        public Task<IListener> CreateListenerAsync(ListenerFactoryContext context)
        {
            throw new NotImplementedException();
        }
    }


    public class EventStreamBackedClassifierTriggerValue
    {

        public string Result { get; set; }
    }

    public class EventStreamBackedClassifierTriggerValueProvider
        : IValueProvider
    {
        public Type Type => throw new NotImplementedException();

        public Task<object> GetValueAsync()
        {
            throw new NotImplementedException();
        }

        public string ToInvokeString()
        {
            throw new NotImplementedException();
        }


    }

    /// <summary>
    /// Class to describe the event stream backed classifier parameter in 
    /// any Azure function with such a parameter designated with the attribute
    /// </summary>
    public class EventStreamBackedClassifierParameterDescriptor 
        : TriggerParameterDescriptor
    {
        public override string GetTriggerReason(IDictionary<string, string> arguments)

        {

            string argumentString = @"";
            if (null != arguments )
            {
                foreach (var argument in arguments )
                {
                    argumentString += $"{argument.Key} = {argument.Value} ";
                }
            }

            return $"Event Stream Backed Classifier trigger fired at { DateTime.Now.ToString("o")}  {argumentString}";

        }

        public EventStreamBackedClassifierParameterDescriptor()
        {
            DisplayHints = new ParameterDisplayHints()
            {
                Description = "Trigger for an event stream backed classifier orchestration",
                Prompt = "Trigger for the specified classifier"
            };
        }

    }

}
