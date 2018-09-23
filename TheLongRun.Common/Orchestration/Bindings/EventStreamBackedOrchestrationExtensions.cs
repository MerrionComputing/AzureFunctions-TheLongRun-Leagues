using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Azure.WebJobs.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
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



            // Add binding rules ... with converters
            // [Command]
            var commandRule = context.AddBindingRule<EventStreamBackedCommandOrchestrationTriggerAttribute>()
                .AddConverter<string, StartCommandOrchestrationArgs>(this.StringToStartCommandArgs)
                .AddConverter<JObject, StartCommandOrchestrationArgs>(this.JObjectToStartCommandArgs); 

            commandRule.BindToInput<EventStreamBackedCommandOrchestrator>(this.GetCommandOrchestration);

            // (2) Orchestrations that can bind to outputs by IAsyncCollector
            //commandRule.BindToCollector<ITableEntity>(builder);

            // [Query]
            var queryRule = context.AddBindingRule<EventStreamBackedQueryOrchestrationTriggerAttribute>()
                .AddConverter<string, StartQueryOrchestrationArgs>(this.StringToStartQueryArgs)
                .AddConverter<JObject, StartQueryOrchestrationArgs>(this.JObjectToStartQueryArgs); 

            queryRule.BindToInput<EventStreamBackedQueryOrchestrator>(this.GetQueryOrchestration);

            // [Idetifier Group]
            var identifierGroupRule = context.AddBindingRule<EventStreamBackedIdentifierGroupOrchestrationTriggerAttribute>()
                .AddConverter<string, StartIdentifierGroupOrchestrationArgs>(this.StringToStartIdentifierGroupArgs)
                .AddConverter<JObject, StartIdentifierGroupOrchestrationArgs>(this.JObjectToStartIdentifierGroupArgs);

            identifierGroupRule.BindToInput<EventStreamBackedIdentifierGroupOrchestrator>(this.GetGroupOrchestration);

            // [Classifier]
            var classifierRule = context.AddBindingRule<EventStreamBackedClassifierOrchestrationTriggerAttribute>()
                .AddConverter<string, StartClassifierOrchestrationArgs>(this.StringToStartClassifierArgs)
                .AddConverter<JObject, StartClassifierOrchestrationArgs>(this.JObjectToStartClassifierArgs);

            classifierRule.BindToInput<EventStreamBackedClassifierOrchestrator>(this.GetClassifierOrchestration);

            // [Projection]
            var projectionRule = context.AddBindingRule<EventStreamBackedProjectionOrchestrationTriggerAttribute>()
                .AddConverter<string, StartProjectionOrchestrationArgs>(this.StringToStartProjectionArgs)
                .AddConverter<JObject, StartProjectionOrchestrationArgs>(this.JObjectToStartProjectionArgs);

            projectionRule.BindToInput<EventStreamBackedProjectionOrchestrator>(this.GetProjectionOrchestration);

            // TODO: IAsyncCollector code to pass on the called orchestrators through...(somehow)
            //projectionRule.BindToCollector<>



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

        protected internal virtual StartCommandOrchestrationArgs StringToStartCommandArgs(string arg)
        {
            if (string.IsNullOrEmpty(arg))
            {
                return null;
            }
            else
            {
                return JsonConvert.DeserializeObject<StartCommandOrchestrationArgs>(arg);
            }
        }

        protected internal virtual StartCommandOrchestrationArgs JObjectToStartCommandArgs(JObject input)
        {
            return input?.ToObject<StartCommandOrchestrationArgs>();
        }

        public class CommandOrchestrationClientAsyncCollector
            : IAsyncCollector<StartCommandOrchestrationArgs>
        {
            public Task AddAsync(StartCommandOrchestrationArgs args,
                CancellationToken cancellationToken = default(CancellationToken))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return Task.CompletedTask;
                }

                // TODO : Run the appropriate command from args. values


                throw new NotImplementedException();
            }


            public Task FlushAsync(CancellationToken cancellationToken = default(CancellationToken))
            {
                return Task.CompletedTask;
            }
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

        protected internal virtual StartQueryOrchestrationArgs StringToStartQueryArgs(string arg)
        {
            if (string.IsNullOrEmpty(arg))
            {
                return null;
            }
            else
            {
                return JsonConvert.DeserializeObject<StartQueryOrchestrationArgs>(arg);
            }
        }

        protected internal virtual StartQueryOrchestrationArgs JObjectToStartQueryArgs(JObject input)
        {
            return input?.ToObject<StartQueryOrchestrationArgs>();
        }


        public class QueryOrchestrationClientAsyncCollector
            : IAsyncCollector<StartQueryOrchestrationArgs>
        {
            public Task AddAsync(StartQueryOrchestrationArgs args,
                CancellationToken cancellationToken = default(CancellationToken))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return Task.CompletedTask;
                }

                // TODO : Run the appropriate query from args. values


                throw new NotImplementedException();
            }


            public Task FlushAsync(CancellationToken cancellationToken = default(CancellationToken))
            {
                return Task.CompletedTask;
            }
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

        protected internal virtual StartIdentifierGroupOrchestrationArgs StringToStartIdentifierGroupArgs(string arg)
        {
            if (string.IsNullOrEmpty(arg))
            {
                return null;
            }
            else
            {
                return JsonConvert.DeserializeObject<StartIdentifierGroupOrchestrationArgs>(arg);
            }
        }

        protected internal virtual StartIdentifierGroupOrchestrationArgs JObjectToStartIdentifierGroupArgs(JObject input)
        {
            return input?.ToObject<StartIdentifierGroupOrchestrationArgs>();
        }

        public class IdentifierGroupOrchestrationClientAsyncCollector
            : IAsyncCollector<StartIdentifierGroupOrchestrationArgs>
        {
            public Task AddAsync(StartIdentifierGroupOrchestrationArgs args,
                CancellationToken cancellationToken = default(CancellationToken))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return Task.CompletedTask;
                }

                // TODO : Run the appropriate identifier group from args. values


                throw new NotImplementedException();
            }


            public Task FlushAsync(CancellationToken cancellationToken = default(CancellationToken))
            {
                return Task.CompletedTask;
            }
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

        protected internal virtual StartClassifierOrchestrationArgs StringToStartClassifierArgs(string arg)
        {
            if (string.IsNullOrEmpty(arg))
            {
                return null;
            }
            else
            {
                return JsonConvert.DeserializeObject<StartClassifierOrchestrationArgs>(arg);
            }
        }

        protected internal virtual StartClassifierOrchestrationArgs JObjectToStartClassifierArgs(JObject input)
        {
            return input?.ToObject<StartClassifierOrchestrationArgs>();
        }

        public class ClassifierOrchestrationClientAsyncCollector
           : IAsyncCollector<StartClassifierOrchestrationArgs>
        {
            public Task AddAsync(StartClassifierOrchestrationArgs args,
                CancellationToken cancellationToken = default(CancellationToken))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return Task.CompletedTask;
                }

                // TODO : Run the appropriate classifier from args. values


                throw new NotImplementedException();
            }


            public Task FlushAsync(CancellationToken cancellationToken = default(CancellationToken))
            {
                return Task.CompletedTask;
            }
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


        protected internal virtual StartProjectionOrchestrationArgs StringToStartProjectionArgs(string arg)
        {
            if (string.IsNullOrEmpty(arg))
            {
                return null;
            }
            else
            {
                return JsonConvert.DeserializeObject<StartProjectionOrchestrationArgs>(arg);
            }
        }

        protected internal virtual StartProjectionOrchestrationArgs JObjectToStartProjectionArgs(JObject input)
        {
            return input?.ToObject<StartProjectionOrchestrationArgs>();
        }

        public class ProjectionOrchestrationClientAsyncCollector
            : IAsyncCollector<StartProjectionOrchestrationArgs>
        {
            public Task AddAsync(StartProjectionOrchestrationArgs args, 
                CancellationToken cancellationToken = default(CancellationToken))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return Task.CompletedTask;
                }

                // TODO : Run the appropriate projection from args. values


                throw new NotImplementedException();
            }


            public Task FlushAsync(CancellationToken cancellationToken = default(CancellationToken))
            {
                return Task.CompletedTask;
            }
        }
        #endregion
    }
}
