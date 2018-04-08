using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Protocols;
using System.Reflection;
using System.Threading.Tasks;


namespace TheLongRun.Common.Bindings
{
    public class ClassifierAttributeBinding
        : IBinding 
    {
        private readonly ParameterInfo _parameter;

        /// <summary>
        /// This binding gets its properties from the Attribute 
        /// </summary>
        public bool FromAttribute
        {
            get { return true; }
        }



        public Task<IValueProvider> BindAsync(BindingContext context)
        {
            return Task.FromResult<IValueProvider>(new ClassifierValueBinder(_parameter));
        }

        public Task<IValueProvider> BindAsync(object value, ValueBindingContext context)
        {
            // TODO: Perform any conversions on the incoming value
            return Task.FromResult<IValueProvider>(new ClassifierValueBinder(_parameter));
        }



        public ParameterDescriptor ToParameterDescriptor()
        {
            return new ParameterDescriptor
            {
                Name = _parameter.Name,
                DisplayHints = new ParameterDisplayHints
                {
                    Description = "The Classifier from which this function can get its data",
                    DefaultValue = "[not applicable]",
                    Prompt = "Please enter a Classifier"
                }
            };
        }

        /// <summary>
        /// Create a new binding for the classifier
        /// </summary>
        /// <param name="parameter">
        /// Details of the attribute and parameter to be bound to
        /// </param>
        public ClassifierAttributeBinding(ParameterInfo parameter)
        {
            _parameter = parameter;
        }
    }
}
