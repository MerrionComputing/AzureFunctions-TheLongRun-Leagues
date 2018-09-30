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
            if (Name.EndsWith("-Classifier"))
            {
                return new FunctionNameAttribute(Name);
            }
            else
            {
                return new FunctionNameAttribute(Name + @"-Classifier");
            }
        }
    }
}
