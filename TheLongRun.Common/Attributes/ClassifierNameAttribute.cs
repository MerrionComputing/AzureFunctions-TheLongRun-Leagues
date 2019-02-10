using Microsoft.Azure.WebJobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheLongRun.Common.Attributes
{
    /// <summary>
    /// The classifiewr name connected to a given classifier implementation function
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class
        , AllowMultiple = false, Inherited = false)]
    public sealed class ClassifierNameAttribute
        : Attribute 
    {
        private readonly string _classifierName;

        public string Name
        {
            get
            {
                return _classifierName;
            }
        }


        public ClassifierNameAttribute(string name)
        {
            _classifierName = name.Trim();
        }


        /// <summary>
        /// Convert this to the default function name to use for a classifier
        /// </summary>
        /// <returns>
        /// This is to allow a more easy to read set of function names in the attribute/code
        /// </returns>
        public FunctionNameAttribute GetDefaultFunctionName()
        {
            return new FunctionNameAttribute(MakeClassifierFunctionName(Name));
        }


        public static string GetClassifierName(Type classifierType)
        {

            ClassifierNameAttribute attribute = (ClassifierNameAttribute)Attribute.GetCustomAttributes(classifierType, typeof(ClassifierNameAttribute)).FirstOrDefault();
            if (null != attribute)
            {
                return attribute.Name;
            }

            // If no attribute found, just return the type name
            return classifierType.Name;
        }

        public static string MakeClassifierFunctionName(string classifierName)
        {
            if (classifierName.EndsWith("-Classifier"))
            {
                return classifierName;
            }
            else
            {
                return classifierName + @"-Classifier";
            }
        }
    }
}
