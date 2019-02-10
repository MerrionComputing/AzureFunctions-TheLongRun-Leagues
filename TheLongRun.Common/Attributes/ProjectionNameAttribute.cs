using Microsoft.Azure.WebJobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheLongRun.Common.Attributes
{

    /// <summary>
    /// The projection name connected to a given projection implementation function
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class
        , AllowMultiple = false, Inherited = false)]
    public sealed class ProjectionNameAttribute
        : Attribute 
    {

        private readonly string _projectionName;

        public string Name
        {
            get
            {
                return _projectionName;
            }
        }


        public ProjectionNameAttribute(string name)
        {
            _projectionName = name.Trim();
        }


        /// <summary>
        /// Convert this to the default function name to use for a projection
        /// </summary>
        /// <returns>
        /// This is to allow a more easy to read set of function names in the attribute/code
        /// </returns>
        public FunctionNameAttribute GetDefaultFunctionName()
        {
            return new FunctionNameAttribute(MakeProjectionFunctionName(_projectionName));
        }

        public static string GetProjectionName(Type projectionType)
        {

            ProjectionNameAttribute attribute = (ProjectionNameAttribute)Attribute.GetCustomAttributes(projectionType, typeof(ProjectionNameAttribute)).FirstOrDefault();
            if (null != attribute )
            {
                return attribute.Name;
            }

            // Try the CQRS projection name attribute
            CQRSAzure.EventSourcing.ProjectionNameAttribute   cqrsattribute = (CQRSAzure.EventSourcing.ProjectionNameAttribute)Attribute.GetCustomAttributes(projectionType, typeof(CQRSAzure.EventSourcing.ProjectionNameAttribute)).FirstOrDefault();
            if (null != cqrsattribute)
            {
                return cqrsattribute.ProjectionName ;
            }

            // If no attribute found, just return the type name
            return projectionType.Name;
        }

        public static string MakeProjectionFunctionName(string projectionName)
        {
            if (projectionName.EndsWith("-Projection"))
            {
                return projectionName;
            }
            else
            {
                return projectionName + @"-Projection";
            }
        }

    }
}
