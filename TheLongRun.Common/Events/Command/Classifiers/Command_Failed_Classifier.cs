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
    /// A classifier to determine whether a command is currently in a failed state 
    /// </summary>
    [CQRSAzure.EventSourcing.DomainNameAttribute("Command")]
    [CQRSAzure.EventSourcing.Category("Command")]
    public sealed class Command_Failed_Classifier
        : IClassifierUntyped,
          IClassifierEventHandler<ProcessingCompleted>,
          IClassifierEventHandler<ValidationErrorOccured>,
          IClassifierEventHandler<ValidationSucceeded >
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

        public IClassifierDataSourceHandler.EvaluationResult EvaluateProjection<TProjection>(TProjection projection) where TProjection : IProjectionUntyped
        {
            throw new NotImplementedException();
        }

        public IClassifierDataSourceHandler.EvaluationResult EvaluateEvent(string eventClassName,
             JObject eventToHandle)
        {
            if (eventClassName == typeof(ValidationSucceeded).Name)
            {
                return EvaluateEvent(eventToHandle.ToObject<ValidationSucceeded>());
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


        public bool HandlesEventType(string eventTypeName)
        {
            if (eventTypeName == typeof(ValidationSucceeded).Name)
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

        public IClassifierDataSourceHandler.EvaluationResult EvaluateEvent(ProcessingCompleted eventToEvaluate)
        {
            return IClassifierDataSourceHandler.EvaluationResult.Exclude;
        }

        public IClassifierDataSourceHandler.EvaluationResult EvaluateEvent(ValidationErrorOccured eventToEvaluate)
        {
            return IClassifierDataSourceHandler.EvaluationResult.Include;
        }

        public IClassifierDataSourceHandler.EvaluationResult EvaluateEvent(ValidationSucceeded eventToEvaluate)
        {
            return IClassifierDataSourceHandler.EvaluationResult.Exclude;
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
