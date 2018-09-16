using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Azure.WebJobs.Logging;
using System;


namespace TheLongRun.Common.Orchestration.Bindings
{
    public class EventStreamBackedOrchestrationExtension
        : IExtensionConfigProvider
    {

        #region Logging
        private static readonly string LoggerCategoryName = LogCategories.CreateTriggerCategory("EventStreamBackedOrchestration");
        #endregion

        /// <summary>
        /// This callback is invoked by the WebJobs framework before the host starts execution.
        /// It adds the binding rules and converters for the event stream backed orchestration
        /// classes
        /// </summary>
        /// <param name="context">
        /// The context to which extensions can be registered
        /// </param>
        public void Initialize(ExtensionConfigContext context)
        {



            // TODO : Add converters...
            // context.AddConverter<SampleItem, string>(ConvertToString);

            // TODO : Add binding rules ...
            // (1) Orhestrations that can bind to inputs
            //    var rule = context.AddBindingRule<SampleAttribute>();
            //    rule.BindToInput<SampleItem>(BuildItemFromAttr);
            // (2) Orchestrations that can bind to outputs by IAsyncCollector
            //    var rule = context.AddBindingRule<TableAttribute>();
            //    rule.BindToCollector<ITableEntity>(builder);
        }
    }
}
