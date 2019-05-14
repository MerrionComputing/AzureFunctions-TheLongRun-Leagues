using System;
using System.Collections;
using System.Collections.Generic;

using CQRSAzure.EventSourcing;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using TheLongRun.Common.Events.Query;

namespace TheLongRun.Common.Events.Query.Projections
{
    /// <summary>
    /// A projection to get the current state of a command based on the events that have occured to it
    /// </summary>
    [CQRSAzure.EventSourcing.DomainNameAttribute(Constants.Domain_Query )]
    [CQRSAzure.EventSourcing.Category("Query")]
    [CQRSAzure.EventSourcing.ProjectionName("Query Projections")]
    public class Query_Summary_Projection
        : CQRSAzure.EventSourcing.ProjectionBaseUntyped,
        CQRSAzure.EventSourcing.IHandleEvent<QueryCreated>,
        CQRSAzure.EventSourcing.IHandleEvent<QueryParameterValueSet>,
        CQRSAzure.EventSourcing.IHandleEvent<QueryParameterValidationErrorOccured>,
        IProjectionUntyped
    {

        #region Private members
        private List<string> parameterNames = new List<string>();
        private ILogger log = null;
        #endregion

        /// <summary>
        /// The different states a query can be in - e.g. to prevent them being processed
        /// in an invalid state
        /// </summary>
        public enum QueryState
        {
            /// <summary>
            /// A new query that has just been created
            /// </summary>
            Created = 0,
            /// <summary>
            /// A query that has been validated and can proceed
            /// </summary>
            Validated = 1,
            /// <summary>
            /// A query marked as invalid
            /// </summary>
            Invalid = 2,
            /// <summary>
            /// A query is being processed 
            /// </summary>
            InProgress = 3,
            /// <summary>
            /// A query marked as complete
            /// </summary>
            Completed = 4
        }

        /// <summary>
        /// Is there a value set for the named parameter?
        /// </summary>
        /// <param name="parameterName">
        /// The name of the parameter
        /// </param>
        public bool ParameterIsSet(string parameterName)
        {
            #region Logging
            if (null != log)
            {
                log.LogDebug($"ParameterIsSet({parameterName}) in {nameof(Query_Summary_Projection )}");
            }
            #endregion
            if (parameterNames.Contains(parameterName))
            {
                object paramValue = base.GetPropertyValue<object>("Parameter." + parameterName);
                if (null != paramValue)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private IDictionary<string, object> Parameters
        {
            get
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                foreach (string paramName in parameterNames)
                {
                    object paramValue = base.GetPropertyValue<object>("Parameter." + paramName);
                    if (null != paramValue)
                    {
                        parameters.Add(paramName, paramValue);
                    }
                }
                return parameters;
            }
        }

        public TParam GetParameter<TParam>(string parameterName)
        {
            if (ParameterIsSet(parameterName))
            {
                return base.GetPropertyValue<TParam>("Parameter." + parameterName);
            }
            else
            {
                return default(TParam);
            }
        }


        /// <summary>
        /// There is no value in storing snapshots for query summaries
        /// </summary>
        public override bool SupportsSnapshots => false;

        #region projection properties

        /// <summary>
        /// The name of the query being executed
        /// </summary>
        public string QueryName
        {
            get
            {
                #region Logging
                if (null != log)
                {
                    log.LogDebug ($"QueryName - Get {nameof(Query_Summary_Projection )}");
                }
                #endregion
                return base.GetPropertyValue<string>(nameof(QueryName));
            }
        }

        public QueryState  CurrentState
        {
            get
            {
                #region Logging
                if (null != log)
                {
                    log.LogDebug ($"CurrentState - Get {nameof(Query_Summary_Projection)}");
                }
                #endregion
                return base.GetPropertyValue<QueryState>(nameof(CurrentState));
            }
        }

        /// <summary>
        /// For queries that rely on authorisation this is the token passed in to test
        /// for the authorisation process
        /// </summary>
        public string AuthorisationToken
        {
            get
            {
                #region Logging
                if (null != log)
                {
                    log.LogDebug ($"AuthorisationToken - Get {nameof(Query_Summary_Projection)}");
                }
                #endregion
                return base.GetPropertyValue<string >(nameof(AuthorisationToken));
            }
        }

        #endregion

        public override void HandleEvent<TEvent>(TEvent eventToHandle)
        {

            #region Logging
            if (null != log)
            {
                log.LogDebug($"HandleEvent<{ typeof(TEvent).FullName  }>()) in {nameof(Query_Summary_Projection )}");
            }
            #endregion

            if (eventToHandle.GetType() == typeof(QueryCreated ))
            {
                HandleEvent(eventToHandle as QueryCreated );
            }

            if (eventToHandle.GetType() == typeof(QueryParameterValueSet ))
            {
                HandleEvent(eventToHandle as QueryParameterValueSet );
            }

            if (eventToHandle.GetType() == typeof(QueryParameterValidationErrorOccured ))
            {
                HandleEvent(eventToHandle as QueryParameterValidationErrorOccured );
            }

            if (eventToHandle.GetType() == typeof(QueryCompleted ))
            {
                HandleEvent(eventToHandle as QueryCompleted );
            }

            if (eventToHandle.GetType() == typeof(ProjectionRequested ))
            {
                HandleEvent(eventToHandle as ProjectionRequested);
            }
        }

        public override bool HandlesEventType(Type eventType)
        {
            return HandlesEventTypeByName(eventType.FullName);
        }

        public override void HandleEventJSon(string eventFullName, JObject eventToHandle)
        {
            #region Logging
            if (null != log)
            {
                log.LogDebug($"HandleEventJSon({eventFullName}) in {nameof(Query_Summary_Projection )}");
            }
            #endregion

            if (eventFullName == typeof(QueryCreated ).FullName)
            {
                HandleEvent<QueryCreated>(eventToHandle.ToObject<QueryCreated>());
            }

            if (eventFullName == typeof(QueryParameterValueSet ).FullName)
            {
                HandleEvent<QueryParameterValueSet>(eventToHandle.ToObject<QueryParameterValueSet>());
            }

            if (eventFullName == typeof(QueryParameterValidationErrorOccured ).FullName)
            {
                HandleEvent<QueryParameterValidationErrorOccured>(eventToHandle.ToObject<QueryParameterValidationErrorOccured>());
            }

            if (eventFullName == typeof(QueryCompleted ).FullName)
            {
                HandleEvent<QueryCompleted>(eventToHandle.ToObject<QueryCompleted>());
            }

            if (eventFullName == typeof(ProjectionRequested).FullName )
            {
                HandleEvent<ProjectionRequested>(eventToHandle.ToObject<ProjectionRequested>()); 
            }

        }

        public override bool HandlesEventTypeByName(string eventTypeFullName)
        {

            #region Logging
            if (null != log)
            {
                log.LogDebug ($"HandlesEventTypeByName({eventTypeFullName}) in {nameof(Query_Summary_Projection)}");
            }
            #endregion

            if (eventTypeFullName == typeof(QueryCreated).FullName)
            {
                return true;
            }

            if (eventTypeFullName == typeof(QueryParameterValueSet ).FullName)
            {
                return true;
            }

            if (eventTypeFullName == typeof(QueryParameterValidationErrorOccured ).FullName)
            {
                return true;
            }

            if (eventTypeFullName == typeof(QueryCompleted ).FullName)
            {
                return true;
            }

            if (eventTypeFullName == typeof(ProjectionRequested).FullName )
            {
                return true;
            }


            return false;
        }


        public  void HandleEvent(QueryCreated eventHandled)
        {
            #region Logging
            if (null != log)
            {
                log.LogDebug ($"HandleEvent( QueryCreated ) in {nameof(Query_Summary_Projection )}");
            }
            #endregion

            if (null != eventHandled)
            {
                // Set the properties from this event
                base.AddOrUpdateValue<string>(nameof(QueryName ), 0, eventHandled.QueryName );
                base.AddOrUpdateValue<string>(nameof(AuthorisationToken ), 0, eventHandled.AuthorisationToken );
                // Set the status as "Created"
                base.AddOrUpdateValue<QueryState >(nameof(CurrentState), 0, QueryState.Created);
                #region Logging
                if (null != log)
                {
                    log.LogDebug ($"Event Handled {eventHandled.QueryName } id: { eventHandled.QueryIdentifier } logged on {eventHandled.Date_Logged }  in {nameof(Query_Summary_Projection )}");
                }
                #endregion
            }
            else
            {
                #region Logging
                if (null != log)
                {
                    log.LogWarning ($"HandleEvent( QueryCreated ) - parameter was null in {nameof(Query_Summary_Projection )}");
                }
                #endregion
            }
        }

        public void HandleEvent(QueryParameterValueSet eventHandled)
        {

            #region Logging
            if (null != log)
            {
                log.LogDebug ($"HandleEvent( ParameterValueSet ) in {nameof(Query_Summary_Projection)}");
            }
            #endregion

            if (null != eventHandled)
            {
                // add or update the parameter value
                string parameterName = @"Parameter." + eventHandled.Name;
                #region Logging
                if (null != log)
                {
                    log.LogDebug ($" Parameter set {parameterName} in {nameof(Query_Summary_Projection)}");
                }
                #endregion
                if (null != eventHandled.Value)
                {
                    base.AddOrUpdateValue(parameterName, 0, eventHandled.Value);

                    if (!parameterNames.Contains(eventHandled.Name))
                    {
                        parameterNames.Add(eventHandled.Name);
                    }

                    #region Logging
                    if (null != log)
                    {
                        log.LogDebug ($" {eventHandled.Name} set to {eventHandled.Value} in {nameof(Query_Summary_Projection)}" );
                    }
                    #endregion
                }
                else
                {
                    // parameter is being cleared
                    if (parameterNames.Contains(eventHandled.Name))
                    {
                        parameterNames.Remove(eventHandled.Name);
                    }
                }
            }
            else
            {
                #region Logging
                if (null != log)
                {
                    log.LogWarning ($"HandleEvent( ParameterValueSet ) - parameter was null in {nameof(Query_Summary_Projection )} ");
                }
                #endregion
            }
        }

        public  void HandleEvent(QueryParameterValidationErrorOccured eventHandled)
        {
            #region Logging
            if (null != log)
            {
                log.LogDebug ($"HandleEvent( QueryParameterValidationErrorOccured ) in {nameof(Query_Summary_Projection)}");
            }
            #endregion

            if (null != eventHandled)
            {
                // Set the status as "In error" if this was a fatal parameter error
                if (eventHandled.Fatal)
                {
                    base.AddOrUpdateValue<QueryState>(nameof(CurrentState), 0, QueryState.Invalid);
                }
                #region Logging
                if (null != log)
                {
                    log.LogDebug ($"Parameter has an invalid value {eventHandled.Name  } Fatal: { eventHandled.Fatal  } Message: {eventHandled.Message  }  in {nameof(Query_Summary_Projection)}");
                }
                #endregion
            }
            else
            {
                #region Logging
                if (null != log)
                {
                    log.LogWarning ($"HandleEvent( QueryParameterValidationErrorOccured ) - parameter was null in {nameof(Query_Summary_Projection)}");
                }
                #endregion
            }
        }

        //ProjectionRequested
        public void HandleEvent(ProjectionRequested eventHandled)
        {
            #region Logging
            if (null != log)
            {
                log.LogDebug($"HandleEvent( ProjectionRequested ) in {nameof(Query_Summary_Projection)}");
            }
            #endregion

            if (null != eventHandled)
            {
                // Set the status as "In error" if this was a fatal parameter error
                base.AddOrUpdateValue<QueryState>(nameof(CurrentState), 0, QueryState.InProgress );
                #region Logging
                if (null != log)
                {
                    log.LogDebug($"Projection requested {eventHandled.ProjectionTypeName} for {eventHandled.AggregateType}-{eventHandled.AggregateInstanceKey } in {nameof(Query_Summary_Projection)}");
                }
                #endregion
            }
            else
            {
                #region Logging
                if (null != log)
                {
                    log.LogWarning($"HandleEvent( QueryParameterValidationErrorOccured ) - parameter was null in {nameof(Query_Summary_Projection)}");
                }
                #endregion
            }
        }

        public Query_Summary_Projection(ILogger logToUse = null)
        {
            if (null != logToUse)
            {
                log = logToUse;
            }
        }
    }


    /// <summary>
    /// Query status returned from the command summary projection
    /// as at a given point in time
    /// </summary>
    [CQRSAzure.EventSourcing.Category(Constants.Domain_Query )]
    public class Query_Summary_Projection_Return
    {

        /// <summary>
        /// The GUID of the unique instance of the query
        /// </summary>
        public string UniqueIdentifier { get; set; }

        /// <summary>
        /// The name of the query being executed
        /// </summary>
        public string QueryName { get; set; }

        /// <summary>
        /// The status of the query as at the current moment
        /// </summary>
        /// <remarks>
        /// This is passed back as a string
        /// </remarks>
        public string Status { get; set; }

        /// <summary>
        /// An external correlation identifier used to identify the query
        /// </summary>
        public string CorrelationIdentifier { get; set; }

        /// <summary>
        /// The as-of date of the last event processed by the projection
        /// </summary>
        public DateTime? AsOfDate { get; set; }

        /// <summary>
        /// The number of the last step completed when this projection was run
        /// </summary>
        public int AsOfStepNumber { get; set; }

    }

    [CQRSAzure.EventSourcing.Category(Constants.Domain_Query)]
    public class Query_Summary_Projection_Request
    {
        /// <summary>
        /// The GUID of the unique instance of the query
        /// </summary>
        public string UniqueIdentifier { get; set; }

        /// <summary>
        /// The name of the query being executed
        /// </summary>
        public string QueryName { get; set; }
    }
}
