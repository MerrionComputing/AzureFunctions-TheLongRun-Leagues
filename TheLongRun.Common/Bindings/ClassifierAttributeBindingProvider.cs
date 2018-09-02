using Microsoft.Azure.WebJobs.Extensions.Bindings;
using Microsoft.Azure.WebJobs.Host.Bindings;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using TheLongRun.Common.Attributes;

namespace TheLongRun.Common.Bindings
{
    public sealed class ClassifierAttributeBindingProvider
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
            ClassifierAttribute attribute = parameter.GetCustomAttribute<ClassifierAttribute>(inherit: false);

            if (attribute == null)
            {
                return Task.FromResult<IBinding>(null);
            }

            // What data type(s) can this attribute be attached to?
            IEnumerable<Type> supportedTypes = new Type[] { typeof(Classifier) };

            if (!(parameter.ParameterType == typeof(Classifier)))
            {
                throw new InvalidOperationException(
                    $"Can't bind ClassifierAttribute to type '{parameter.ParameterType}'.");
            }

            return Task.FromResult<IBinding>(new ClassifierAttributeBinding(parameter));

        }
    }
}
