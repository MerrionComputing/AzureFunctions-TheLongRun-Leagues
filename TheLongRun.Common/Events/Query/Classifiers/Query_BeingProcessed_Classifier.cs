using System;
using System.Collections;
using System.Collections.Generic;

using CQRSAzure.EventSourcing;
using CQRSAzure.EventSourcing.IdentityGroups;
using CQRSAzure.IdentifierGroup;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json.Linq;
using TheLongRun.Common.Events.Query;

namespace TheLongRun.Common.Events.Query.Classifiers
{
    /// <summary>
    /// A classifier to determine whether a query is currently being processed
    /// </summary>
    [CQRSAzure.EventSourcing.DomainNameAttribute("Query")]
    [CQRSAzure.EventSourcing.Category("Query")]
    public sealed class Query_BeingProcessed_Classifier
          : IClassifierUntyped,
          IClassifierEventHandler<QueryCreated>,
          IClassifierEventHandler<QueryParameterValidationErrorOccured>,
          IClassifierEventHandler<QueryCompleted>
    {

        /// <summary>
        /// This classifier does not support snapshots as the number of events in a query is typically too small
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

        public bool HandlesEventType(string eventTypeName)
        {
            if (eventTypeName == typeof(QueryCreated).Name)
            {
                return true;
            }
            if (eventTypeName == typeof(QueryParameterValidationErrorOccured).Name)
            {
                return true;
            }
            if (eventTypeName == typeof(QueryCompleted).Name)
            {
                return true;
            }
            return false;
        }

        public IClassifierDataSourceHandler.EvaluationResult EvaluateEvent(string eventClassName,
        JObject eventToHandle)
        {

            if (eventClassName == typeof(QueryCreated).Name)
            {
                return EvaluateEvent(eventToHandle.ToObject<QueryCreated>());
            }

            if (eventClassName == typeof(QueryParameterValidationErrorOccured).Name)
            {
                return EvaluateEvent(eventToHandle.ToObject<QueryParameterValidationErrorOccured>());
            }

            if (eventClassName == typeof(QueryCompleted).Name)
            {
                return EvaluateEvent(eventToHandle.ToObject<QueryCompleted>());
            }

            return IClassifierDataSourceHandler.EvaluationResult.Unchanged;
        }

        public IClassifierDataSourceHandler.EvaluationResult EvaluateEvent(QueryCompleted eventToEvaluate)
        {
            // Completed queries are not in the group
            return IClassifierDataSourceHandler.EvaluationResult.Exclude;
        }

        public IClassifierDataSourceHandler.EvaluationResult EvaluateEvent(QueryCreated eventToEvaluate)
        {
            // Created queries are in the being processed group
            return IClassifierDataSourceHandler.EvaluationResult.Include;
        }

        public IClassifierDataSourceHandler.EvaluationResult EvaluateEvent(QueryParameterValidationErrorOccured eventToEvaluate)
        {
            // Queries in error are not in the group
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
