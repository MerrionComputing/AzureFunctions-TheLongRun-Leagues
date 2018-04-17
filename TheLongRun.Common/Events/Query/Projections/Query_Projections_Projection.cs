using System;
using System.Collections;
using System.Collections.Generic;

using CQRSAzure.EventSourcing;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json.Linq;
using TheLongRun.Common.Events.Query;

namespace TheLongRun.Common.Events.Query.Projections
{
    /// <summary>
    /// A projection to list the requested and executed projections for this query
    /// </summary>
    public class Query_Projections_Projection
        : CQRSAzure.EventSourcing.ProjectionBaseUntyped,
        CQRSAzure.EventSourcing.IHandleEvent<ProjectionRequested >,
        CQRSAzure.EventSourcing.IHandleEvent<ProjectionValueReturned >,
        IProjectionUntyped
    {

        #region Private members
        private TraceWriter log = null;
        private List<ProjectionRequested> unprocessedRequests = new List<ProjectionRequested>();
        private List<ProjectionValueReturned> processedRequests = new List<ProjectionValueReturned>();
        #endregion

        /// <summary>
        /// The set of projection requests that have to yet to be run
        /// </summary>
        public IReadOnlyList<ProjectionRequested > UnprocessedRequests
        {
            get
            {
                return unprocessedRequests.AsReadOnly();
            }
        }

        /// <summary>
        /// The set of requests that have been processed
        /// </summary>
        public IReadOnlyList <ProjectionValueReturned > ProcessedRequests
        {
            get
            {
                return processedRequests.AsReadOnly();
            }
        }

        /// <summary>
        /// There is no value in storing snapshots for query summaries
        /// </summary>
        public override bool SupportsSnapshots => false;


        public override void HandleEventJSon(string eventFullName, JObject eventToHandle)
        {
            #region Logging
            if (null != log)
            {
                log.Verbose($"HandleEventJSon({eventFullName})",
                    nameof(Query_Summary_Projection));
            }
            #endregion

            if (eventFullName == typeof(ProjectionRequested ).FullName)
            {
                HandleEvent<ProjectionRequested>(eventToHandle.ToObject<ProjectionRequested>());
            }

            if (eventFullName == typeof(ProjectionValueReturned ).FullName)
            {
                HandleEvent<ProjectionValueReturned>(eventToHandle.ToObject<ProjectionValueReturned>());
            }

        }

        public override bool HandlesEventType(Type eventType)
        {
            return HandlesEventTypeByName(eventType.FullName);
        }

        public override bool HandlesEventTypeByName(string eventTypeFullName)
        {

            #region Logging
            if (null != log)
            {
                log.Verbose($"HandlesEventTypeByName({eventTypeFullName})",
                    nameof(Query_Projections_Projection ));
            }
            #endregion


            if (eventTypeFullName == typeof(ProjectionRequested  ).FullName)
            {
                return true;
            }

            if (eventTypeFullName == typeof(ProjectionValueReturned).FullName)
            {
                return true;
            }

            return false;
        }

        public override void HandleEvent<TEvent>(TEvent eventToHandle)
        {

            #region Logging
            if (null != log)
            {
                log.Verbose($"HandleEvent<{ typeof(TEvent).FullName  }>())",
                    nameof(Query_Projections_Projection ));
            }
            #endregion

            if (eventToHandle.GetType() == typeof(ProjectionRequested ))
            {
                HandleEvent(eventToHandle as ProjectionRequested);
            }

            if (eventToHandle.GetType() == typeof(ProjectionValueReturned ))
            {
                HandleEvent(eventToHandle as ProjectionValueReturned );
            }
        }

        /// <summary>
        /// A projection was requested for this query
        /// </summary>
        /// <param name="eventHandled">
        /// Detail of the projection requested
        /// </param>
        public  void HandleEvent(ProjectionRequested eventHandled)
        {
            if (null != eventHandled )
            {
                #region Logging
                if (null != log)
                {
                    log.Verbose($"Query projection requested  {eventHandled.ProjectionTypeName  } for { eventHandled.DomainName }.{eventHandled.AggregateType}.{eventHandled.AggregateInstanceKey } as at {eventHandled.AsOfDate} ",
                        nameof(Query_Projections_Projection ));
                }
                #endregion
                // add this to the unprocessed list
                unprocessedRequests.Add(eventHandled); 
            }
        }

        /// <summary>
        /// A projection was evaluated and the results returned for this query
        /// </summary>
        /// <param name="eventHandled">
        /// Detail of the projection value returned
        /// </param>
        public  void HandleEvent(ProjectionValueReturned eventHandled)
        {
            if (null != eventHandled)
            {
                #region Logging
                if (null != log)
                {
                    log.Verbose($"Query projection value returned  {eventHandled.ProjectionTypeName  } for { eventHandled.DomainName }.{eventHandled.AggregateType}.{eventHandled.AggregateInstanceKey } as at {eventHandled.AsOfDate} ",
                        nameof(Query_Projections_Projection));
                }
                #endregion
                // remove this from this to the unprocessed list
                int unprocessedIndex = unprocessedRequests.FindIndex(f =>
                f.DomainName == eventHandled.DomainName &&
                f.AggregateType == eventHandled.AggregateType &&
                f.AggregateInstanceKey == eventHandled.AggregateInstanceKey);

                if (unprocessedIndex >= 0)
                {
                    unprocessedRequests.RemoveAt(unprocessedIndex);
                }
                // add it to the processed list
                processedRequests.Add(eventHandled);
            }
        }

        public Query_Projections_Projection(TraceWriter logIn = null)
        {
            if (null != logIn )
            {
                log = logIn;
            }
        }
    }
}
