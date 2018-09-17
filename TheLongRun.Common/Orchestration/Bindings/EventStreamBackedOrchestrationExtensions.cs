using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Azure.WebJobs.Logging;
using System;
using System.Collections.Concurrent;
using TheLongRun.Common.Orchestration.Attributes;

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
            // [Command]
            // [Query]
            // [Idetifier Group]
            // [Classifier]
            // [Projection]

            // TODO : Add binding rules ...
            // (1) Orhestrations that can bind to inputs
            //    var rule = context.AddBindingRule<SampleAttribute>();
            //    rule.BindToInput<SampleItem>(BuildItemFromAttr);
            // [Command]
            var commandRule = context.AddBindingRule<EventStreamBackedCommandOrchestrationTriggerAttribute>();
            commandRule.BindToInput<EventStreamBackedCommandOrchestrator>(this.GetCommandOrchestration);

            // [Query]
            var queryRule = context.AddBindingRule<EventStreamBackedQueryOrchestrationTriggerAttribute>();
            queryRule.BindToInput<EventStreamBackedQueryOrchestrator>(this.GetQueryOrchestration);

            // [Idetifier Group]
            var identifierGroupRule = context.AddBindingRule<EventStreamBackedIdentifierGroupOrchestrationTriggerAttribute>();
            identifierGroupRule.BindToInput<EventStreamBackedIdentifierGroupOrchestrator>(this.GetGroupOrchestration);

            // [Classifier]
            var classifierRule = context.AddBindingRule<EventStreamBackedClassifierOrchestrationTriggerAttribute>();
            classifierRule.BindToInput<EventStreamBackedClassifierOrchestrator>(this.GetClassifierOrchestration);

            // [Projection]
            var projectionRule = context.AddBindingRule<EventStreamBackedProjectionOrchestrationTriggerAttribute>();
            projectionRule.BindToInput<EventStreamBackedProjectionOrchestrator>(this.GetProjectionOrchestration); 

            // (2) Orchestrations that can bind to outputs by IAsyncCollector
            //    var rule = context.AddBindingRule<TableAttribute>();
            //    rule.BindToCollector<ITableEntity>(builder);
            // [Command]
            // [Query]
            // [Idetifier Group]
            // [Classifier]
            // [Projection]

        }

        #region Command
        /// <summary>
        /// A cache of the command handlers that have been created in this session (to avoid the overhead of creating new each time)
        /// </summary>
        private readonly ConcurrentDictionary<EventStreamBackedCommandOrchestrationTriggerAttribute, EventStreamBackedCommandOrchestrator> cachedCommandOrchestrators =
            new ConcurrentDictionary<EventStreamBackedCommandOrchestrationTriggerAttribute, EventStreamBackedCommandOrchestrator>();


        protected internal virtual EventStreamBackedCommandOrchestrator GetCommandOrchestration(EventStreamBackedCommandOrchestrationTriggerAttribute attribute)
        {
            EventStreamBackedCommandOrchestrator commandOrchestration = this.cachedCommandOrchestrators.GetOrAdd(
                attribute,
                attr =>
                {
                    // TODO :: Need to get the domain context to go along with the command
                    return  EventStreamBackedCommandOrchestrator.CreateFromAttribute(attribute );
                });

            return commandOrchestration;
        }
        #endregion

        #region Query
        /// <summary>
        /// A cache of the query handlers that have been created in this session (to avoid the overhead of creating new each time)
        /// </summary>
        private readonly ConcurrentDictionary<EventStreamBackedQueryOrchestrationTriggerAttribute, EventStreamBackedQueryOrchestrator> cachedQueryOrchestrators =
            new ConcurrentDictionary<EventStreamBackedQueryOrchestrationTriggerAttribute, EventStreamBackedQueryOrchestrator>();


        protected internal virtual EventStreamBackedQueryOrchestrator GetQueryOrchestration(EventStreamBackedQueryOrchestrationTriggerAttribute attribute)
        {
            EventStreamBackedQueryOrchestrator queryOrchestration = this.cachedQueryOrchestrators.GetOrAdd(
                attribute,
                attr =>
                {
                    // TODO :: Need to get the domain context to go along with the command
                    return EventStreamBackedQueryOrchestrator.CreateFromAttribute(attribute);
                });

            return queryOrchestration;
        }
        #endregion

        #region Identifier Group
        /// <summary>
        /// A cache of the identifier group handlers that have been created in this session (to avoid the overhead of creating new each time)
        /// </summary>
        private readonly ConcurrentDictionary<EventStreamBackedIdentifierGroupOrchestrationTriggerAttribute, EventStreamBackedIdentifierGroupOrchestrator> cachedGroupOrchestrators =
            new ConcurrentDictionary<EventStreamBackedIdentifierGroupOrchestrationTriggerAttribute, EventStreamBackedIdentifierGroupOrchestrator>();


        protected internal virtual EventStreamBackedIdentifierGroupOrchestrator GetGroupOrchestration(EventStreamBackedIdentifierGroupOrchestrationTriggerAttribute attribute)
        {
            EventStreamBackedIdentifierGroupOrchestrator groupOrchestration = this.cachedGroupOrchestrators.GetOrAdd(
                attribute,
                attr =>
                {
                    // TODO :: Need to get the domain context to go along with the command
                    return EventStreamBackedIdentifierGroupOrchestrator.CreateFromAttribute(attribute);
                });

            return groupOrchestration;
        }
        #endregion

        #region Classifier
        /// <summary>
        /// A cache of the identifier group handlers that have been created in this session (to avoid the overhead of creating new each time)
        /// </summary>
        private readonly ConcurrentDictionary<EventStreamBackedClassifierOrchestrationTriggerAttribute, EventStreamBackedClassifierOrchestrator> cachedClassifierOrchestrators =
            new ConcurrentDictionary<EventStreamBackedClassifierOrchestrationTriggerAttribute, EventStreamBackedClassifierOrchestrator>();


        protected internal virtual EventStreamBackedClassifierOrchestrator GetClassifierOrchestration(EventStreamBackedClassifierOrchestrationTriggerAttribute attribute)
        {
            EventStreamBackedClassifierOrchestrator classifierOrchestration = this.cachedClassifierOrchestrators.GetOrAdd(
                attribute,
                attr =>
                {
                    // TODO :: Need to get the domain context to go along with the command
                    return EventStreamBackedClassifierOrchestrator.CreateFromAttribute(attribute);
                });

            return classifierOrchestration;
        }
        #endregion

        #region Projection
        //
        /// <summary>
        /// A cache of the identifier group handlers that have been created in this session (to avoid the overhead of creating new each time)
        /// </summary>
        private readonly ConcurrentDictionary<EventStreamBackedProjectionOrchestrationTriggerAttribute, EventStreamBackedProjectionOrchestrator> cachedProjectionOrchestrators =
            new ConcurrentDictionary<EventStreamBackedProjectionOrchestrationTriggerAttribute, EventStreamBackedProjectionOrchestrator>();

        protected internal virtual EventStreamBackedProjectionOrchestrator GetProjectionOrchestration(EventStreamBackedProjectionOrchestrationTriggerAttribute attribute)
        {
            EventStreamBackedProjectionOrchestrator projectionOrchestration = this.cachedProjectionOrchestrators.GetOrAdd(
                attribute,
                attr =>
                {
                    // TODO :: Need to get the domain context to go along with the command
                    return EventStreamBackedProjectionOrchestrator.CreateFromAttribute(attribute);
                });

            return projectionOrchestration;
        }
        #endregion
    }
}
