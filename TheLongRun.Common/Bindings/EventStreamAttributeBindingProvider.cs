using Microsoft.Azure.WebJobs.Extensions.Bindings;
using Microsoft.Azure.WebJobs.Host.Bindings;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using TheLongRun.Common.Attributes;

namespace TheLongRun.Common.Bindings
{



    /// <summary>
    /// Output binding provider to select the event stream on which to append 
    /// events or run projections or classifiers.
    /// </summary>
    public sealed class EventStreamAttributeBindingProvider
        : IBindingProvider
    {


        public Task<IBinding> TryCreateAsync(BindingProviderContext context)
        {

            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            // Determine whether we should bind to the current parameter
            ParameterInfo parameter = context.Parameter;
            EventStreamAttribute attribute = parameter.GetCustomAttribute<EventStreamAttribute>(inherit: false);

            if (attribute == null)
            {
                return Task.FromResult<IBinding>(null);
            }

            // This can only bind to EventStream
            IEnumerable<Type> supportedTypes = new Type[] { typeof(EventStream ) };

            if (!ValueBinder.MatchParameterType(context.Parameter, supportedTypes))
            {
                throw new InvalidOperationException(
                        $"Can't bind EventStreamAttribute to type '{parameter.ParameterType}'.");
            }

            return Task.FromResult<IBinding>(new EventStreamAttributeBinding(parameter));

        }
    }

}
