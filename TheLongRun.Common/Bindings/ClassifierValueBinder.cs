using Microsoft.Azure.WebJobs.Host.Bindings;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using TheLongRun.Common.Attributes;

namespace TheLongRun.Common.Bindings
{
    public sealed class ClassifierValueBinder
        : IValueBinder 
    {
        private readonly ParameterInfo _parameter;

        public Type Type
        {
            get
            {
                return typeof(Bindings.Classifier);
            }
        }



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
                ClassifierAttribute attribute = _parameter.GetCustomAttribute<ClassifierAttribute >(inherit: false);
                if (null != attribute)
                {
                    item = new Classifier(attribute);
                }
            }

            return item;
        }

        /// <summary>
        /// This will be expanded out to make sure the domain, aggregate and classifier type really exist,
        /// and are mapped
        /// </summary>
        /// <param name="parameter">
        /// The classifier parameter
        /// </param>
        /// <returns></returns>
        private Task ValidateParameter(ParameterInfo parameter)
        {
            return Task.CompletedTask;
        }

        public ClassifierValueBinder(ParameterInfo parameter)
        {
            _parameter = parameter;
        }
    }
}
