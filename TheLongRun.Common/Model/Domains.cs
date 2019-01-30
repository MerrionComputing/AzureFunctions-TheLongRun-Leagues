using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace TheLongRun.Common.Model
{
    /// <summary>
    /// The top level of the domains model functionality
    /// </summary>
    /// <remarks>
    /// This allows for the domain(s) models to be loaded - either from code or by reflection over existing business 
    /// logic layer libraries marked with the CQRSAzure attributes.
    /// </remarks>
    public  class Domains
    {

        private  readonly Dictionary<string, Domain> _domains;


        /// <summary>
        /// Indexer to get the entity type by name from this collection
        /// </summary>
        /// <param name="index">
        /// The name of the entity type
        /// </param>
        public Domain  this[string index]
        {
            get
            {
                if (_domains .ContainsKey(index))
                {
                    return _domains[index];
                }
                return null;
            }
        }

        /// <summary>
        /// Add the domain if it has not already been added
        /// </summary>
        /// <param name="domainToAdd">
        /// The new domain definition to add to this model
        /// </param>
        public void AddDomain(Domain domainToAdd)
        {
            if (! _domains.ContainsKey(domainToAdd.Name )  )
            {
                _domains.Add(domainToAdd.Name, domainToAdd);
            }
        }

        /// <summary>
        /// Load the domain model information contained in the given assembly, using reflection and the CQRSAzure library attributes
        /// </summary>
        /// <param name="sourceAssemly">
        /// The assembly from which to load the domain(s) model
        /// </param>
        /// <remarks>
        /// This does not clear any existing model so can be called multiple times to load a model contained in multiple domains
        /// </remarks>
        public void LoadFromAssembly(Assembly sourceAssemly)
        {
            throw new NotImplementedException();
        }

        public  Domains()
        {
            _domains = new Dictionary<string, Domain>();
        }

    }

    /// <summary>
    /// Methods for constructing the domain model usin a fluent syntax
    /// </summary>
    public static partial class FluentInterface
    {

        /// <summary>
        /// Add a domain to the model domains
        /// </summary>
        /// <param name="domains">
        /// The set of domains to add this new one to
        /// </param>
        /// <param name="domainToAdd">
        /// The new domain to add
        /// </param>
        /// <returns></returns>
        public static Domains Add(this Domains domains, Domain domainToAdd)
        {
            domains.AddDomain(domainToAdd);
            return domains;
        }

    }
}
