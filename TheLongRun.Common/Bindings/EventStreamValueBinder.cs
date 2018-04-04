using Microsoft.Azure.WebJobs.Host.Bindings;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using TheLongRun.Common.Attributes;

namespace TheLongRun.Common.Bindings
{
    public class EventStreamValueBinder
        : IValueBinder 
    {

        private readonly ParameterInfo _parameter;

        public Type Type {
            get
            {
                return typeof(EventStream);
            }
        }

        /*
        /// <summary>
        /// Turn the parameter (with attribute) into an EventStream object
        /// </summary>
        /// <returns>
        /// A built [EventStream] object
        /// </returns>
        public override Task<object> GetValueAsync()
        {
            object item = null;

            if (null != _parameter)
            {
                EventStreamAttribute attribute = _parameter.GetCustomAttribute<EventStreamAttribute>(inherit: false);
                if (null != attribute)
                { 
                    item = new EventStream(attribute);
                }
            }

            return item;
        }

    */

        public string ToInvokeString()
        {
            return string.Empty;
        }

        public Task SetValueAsync(object value, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<object> GetValueAsync()
        {

            object item = null;

            await ValidateParameter(_parameter);

            if (null != _parameter)
            {
                EventStreamAttribute attribute = _parameter.GetCustomAttribute<EventStreamAttribute>(inherit: false);
                if (null != attribute)
                {
                    item = new EventStream(attribute);
                }
            }

            return item;
        }

        /// <summary>
        /// This will be expanded out to make sure the domain and aggregate really exist,
        /// and are mapped
        /// </summary>
        /// <param name="parameter">
        /// The EventStream parameter
        /// </param>
        /// <returns></returns>
        private Task ValidateParameter(ParameterInfo parameter)
        {
            return Task.CompletedTask;
        }

        public EventStreamValueBinder(ParameterInfo parameter)
        { 
            _parameter = parameter;
        }
    }
}
