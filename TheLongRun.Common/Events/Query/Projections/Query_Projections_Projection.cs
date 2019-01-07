using System;
using System.Collections;
using System.Collections.Generic;

using CQRSAzure.EventSourcing;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using TheLongRun.Common.Attributes;
using TheLongRun.Common.Events.Query;

namespace TheLongRun.Common.Events.Query.Projections
{
    /// <summary>
    /// A projection to list the requested and executed projections for this query
    /// </summary>
    [TheLongRun.Common.Attributes.ProjectionName("Query Projections")]
    public class Query_Projections_Projection
        : CQRSAzure.EventSourcing.ProjectionBaseUntyped,
        CQRSAzure.EventSourcing.IHandleEvent<ProjectionRequested >,
        CQRSAzure.EventSourcing.IHandleEvent<ProjectionRunStarted >,
        CQRSAzure.EventSourcing.IHandleEvent<ProjectionValueReturned >,
        IProjectionUntyped
    {

        #region Private members
        private ILogger log = null;
        private List<ProjectionRequested> unprocessedRequests = new List<ProjectionRequested>();
        private List<ProjectionRunStarted> inflightRequests = new List<ProjectionRunStarted>();
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
        /// The set of projection requests being run
        /// </summary>
        public IReadOnlyList<ProjectionRunStarted > RequestsInProgress
        {
            get
            {
                return inflightRequests.AsReadOnly();
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
                log.LogDebug ($"HandleEventJSon({eventFullName}) in {nameof(Query_Summary_Projection)}");
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
                log.LogDebug ($"HandlesEventTypeByName({eventTypeFullName}) in {nameof(Query_Projections_Projection )}");
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

            if (eventTypeFullName == typeof(ProjectionRunStarted).FullName)
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
                log.LogDebug ($"HandleEvent<{ typeof(TEvent).FullName  }>()) in {nameof(Query_Projections_Projection )}");
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

            if (eventToHandle.GetType() == typeof(ProjectionRunStarted ))
            {
                HandleEvent(eventToHandle as ProjectionRunStarted);
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
                    log.LogDebug ($"Query projection requested  {eventHandled.ProjectionTypeName  } for { eventHandled.DomainName }.{eventHandled.AggregateType}.{eventHandled.AggregateInstanceKey } as at {eventHandled.AsOfDate}  in {nameof(Query_Projections_Projection )}");
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
                    log.LogDebug ($"Query projection value returned  {eventHandled.ProjectionTypeName  } for { eventHandled.DomainName }.{eventHandled.AggregateType}.{eventHandled.AggregateInstanceKey } as at {eventHandled.AsOfDate} in { nameof(Query_Projections_Projection)}");
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

                // remove it from any in-progress list
                int inflightIndex = inflightRequests.FindIndex(f =>
                f.DomainName == eventHandled.DomainName &&
                f.AggregateType == eventHandled.AggregateType &&
                f.AggregateInstanceKey == eventHandled.AggregateInstanceKey);

                if (inflightIndex >= 0)
                {
                    inflightRequests.RemoveAt(inflightIndex);
                }

                // add it to the processed list
                processedRequests.Add(eventHandled);
            }
        }

        public  void HandleEvent(ProjectionRunStarted eventHandled)
        {
            if (null != eventHandled)
            {
                #region Logging
                if (null != log)
                {
                    log.LogDebug ($"Query projection run started  {eventHandled.ProjectionTypeName  } for { eventHandled.DomainName }.{eventHandled.AggregateType}.{eventHandled.AggregateInstanceKey } as at {eventHandled.AsOfDate} in {nameof(Query_Projections_Projection)}");
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

                // add it to the in-flight requests list
                inflightRequests.Add(eventHandled);
            }
        }

        public Query_Projections_Projection(ILogger logIn = null)
        {
            if (null != logIn)
            {
                log = logIn;
            }
        }
    }


    /// <summary>
    /// A single projection status returned from running the "Query Projections" projection
    /// </summary>
    [CQRSAzure.EventSourcing.Category(Constants.Domain_Query)]
    public class Query_Projections_Projection_Return
    {

        /// <summary>
        /// The state of the projection run on this query
        /// </summary>
        public enum QueryProjectionState
        {
            /// <summary>
            /// The projection has been requested but not started
            /// </summary>
            Queued = 0,
            /// <summary>
            /// The projection is currently running
            /// </summary>
            InProgress = 1,
            /// <summary>
            /// The projection is complete and the results are on the query event stream
            /// </summary>
            Complete = 2
        }


        /// <summary>
        /// The state of the projection as at the time the request was run
        /// </summary>
        public QueryProjectionState ProjectionState { get; set; }

        /// <summary>
        /// The detail of the projection request
        /// </summary>
        public ProjectionAttribute Projection { get; set; }
    }


    [CQRSAzure.EventSourcing.Category(Constants.Domain_Query)]
    public class Query_Projections_Projection_Request
    {
        /// <summary>
        /// The GUID of the unique instance of the query
        /// </summary>
        public string UniqueIdentifier { get; set; }

        /// <summary>
        /// Th ename of the query we want to list the projections for
        /// </summary>
        /// <remarks>
        /// This is required as the different query types may have different backing store locations
        /// </remarks>
        public string QueryName { get; set; }
    }
}
