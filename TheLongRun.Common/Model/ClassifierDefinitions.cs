using System;
using System.Collections.Generic;
using System.Text;

namespace TheLongRun.Common.Model
{
    /// <summary>
    /// The set of classifier definitions for a domain entity type
    /// </summary>
    public class ClassifierDefinitions
    {

        private readonly Dictionary<string, ClassifierDefinition> _classifierDefinitions;

        /// <summary>
        /// Indexer to get the classifier by name from this collection
        /// </summary>
        /// <param name="index">
        /// The name of the classifier
        /// </param>
        public ClassifierDefinition this[string index]
        {
            get
            {
                if (_classifierDefinitions.ContainsKey(index))
                {
                    return _classifierDefinitions[index];
                }
                return null;
            }
        }

        internal void AddClassifierDefinition(ClassifierDefinition classifierToAdd)
        {
            if (!_classifierDefinitions.ContainsKey(classifierToAdd.Name))
            {
                _classifierDefinitions.Add(classifierToAdd.Name, classifierToAdd);
            }
        }

        public ClassifierDefinitions()
        {
            _classifierDefinitions = new Dictionary<string, ClassifierDefinition>();
        }


    }

    /// <summary>
    /// Methods for constructing the domain model using a fluent syntax
    /// </summary>
    public static partial class FluentInterface
    {

        /// <summary>
        /// Add a classifier definition to the collection
        /// </summary> add
        /// <returns></returns>
        public static ClassifierDefinitions  Add(this ClassifierDefinitions classifierDefinitions , ClassifierDefinition  classifierDefinitionToAdd )
        {
            classifierDefinitions.AddClassifierDefinition(classifierDefinitionToAdd);
            return classifierDefinitions;
        }

    }
}
