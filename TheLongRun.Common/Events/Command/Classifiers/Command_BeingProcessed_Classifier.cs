using System;
using System.Collections;
using System.Collections.Generic;

using CQRSAzure.EventSourcing;
using CQRSAzure.EventSourcing.IdentityGroups;
using CQRSAzure.IdentifierGroup;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json.Linq;
using TheLongRun.Common.Events.Command;

namespace TheLongRun.Common.Events.Command.Classifiers
{
    /// <summary>
    /// A classifier to determine whether a command is currently being processed
    /// </summary>
    [CQRSAzure.EventSourcing.DomainNameAttribute("Command")]
    [CQRSAzure.EventSourcing.Category("Command")]
    public sealed class Command_BeingProcessed_Classifier
        : IClassifierUntyped,
          IClassifierEventHandler<ProcessingCompleted>,
          IClassifierEventHandler<CommandCreated>,
          IClassifierEventHandler<ValidationErrorOccured>
    {

        /// <summary>
        /// This classifier does not support snapshots as the number of events in a command is typically too small
        /// to make that worthwhile
        /// </summary>
        public bool SupportsSnapshots => false;

        /// <summary>
        /// This classifier gets its setting from reading the event stream 
        /// </summary>
        public IClassifier.ClassifierDataSourceType ClassifierDataSource => IClassifier.ClassifierDataSourceType.EventHandler;


        public IClassifierDataSourceHandler.EvaluationResult EvaluateEvent(string eventClassName, 
            JObject eventToHandle)
        {
            if (eventClassName == typeof(CommandCreated).Name)
            {
                return EvaluateEvent(eventToHandle.ToObject<CommandCreated>());
            }

            if (eventClassName == typeof(ValidationErrorOccured).Name)
            {
                return EvaluateEvent(eventToHandle.ToObject<ValidationErrorOccured>());
            }

            if (eventClassName == typeof(ProcessingCompleted).Name)
            {
                return EvaluateEvent(eventToHandle.ToObject<ProcessingCompleted>());
            }

            return IClassifierDataSourceHandler.EvaluationResult.Unchanged;
        }

        public IClassifierDataSourceHandler.EvaluationResult EvaluateEvent(ProcessingCompleted eventToEvaluate)
        {
            // Completed commands are not "being processed"
            return IClassifierDataSourceHandler.EvaluationResult.Exclude;
        }

        public IClassifierDataSourceHandler.EvaluationResult EvaluateEvent(CommandCreated eventToEvaluate)
        {
            // Created commands are in process
            return IClassifierDataSourceHandler.EvaluationResult.Include;
        }

        public IClassifierDataSourceHandler.EvaluationResult EvaluateEvent(ValidationErrorOccured eventToEvaluate)
        {
            // Commands that have validation errors are not "being processed"
            return IClassifierDataSourceHandler.EvaluationResult.Exclude;
        }

        public IClassifierDataSourceHandler.EvaluationResult EvaluateProjection<TProjection>(TProjection projection) where TProjection : IProjectionUntyped
        {
            throw new NotImplementedException();
        }

        public bool HandlesEventType(string eventTypeName)
        {
            if (eventTypeName == typeof(CommandCreated).Name)
            {
                return true;
            }
            if (eventTypeName == typeof(ValidationErrorOccured).Name)
            {
                return true;
            }
            if (eventTypeName == typeof(ProcessingCompleted).Name)
            {
                return true;
            }
            return false;
        }

        #region "Snapshot"
        public void LoadFromSnapshot(IClassifierSnapshotUntyped latestSnapshot)
        {
            throw new NotImplementedException();
        }

        public IClassifierSnapshotUntyped ToSnapshot()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
