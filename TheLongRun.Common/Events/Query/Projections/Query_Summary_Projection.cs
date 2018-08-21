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
    /// A projection to get the current state of a command based on the events that have occured to it
    /// </summary>
    [CQRSAzure.EventSourcing.DomainNameAttribute("Query")]
    [CQRSAzure.EventSourcing.Category("Query")]
    public class Query_Summary_Projection
        : CQRSAzure.EventSourcing.ProjectionBaseUntyped,
        CQRSAzure.EventSourcing.IHandleEvent<QueryCreated>,
        CQRSAzure.EventSourcing.IHandleEvent<QueryParameterValueSet>,
        CQRSAzure.EventSourcing.IHandleEvent<QueryParameterValidationErrorOccured>,
        IProjectionUntyped
    {

        #region Private members
        private List<string> parameterNames = new List<string>();
        private TraceWriter log = null;
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
                log.Verbose($"ParameterIsSet({parameterName}) ",
                    nameof(Query_Summary_Projection ));
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
                    log.Verbose($"CommandName - Get ",
                        nameof(Query_Summary_Projection ));
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
                    log.Verbose($"CurrentState - Get ",
                        nameof(Query_Summary_Projection ));
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
                    log.Verbose($"AuthorisationToken - Get ",
                        nameof(Query_Summary_Projection));
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
                log.Verbose($"HandleEvent<{ typeof(TEvent).FullName  }>())",
                    nameof(Query_Summary_Projection ));
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
                log.Verbose($"HandleEventJSon({eventFullName})",
                    nameof(Query_Summary_Projection ));
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


        }

        public override bool HandlesEventTypeByName(string eventTypeFullName)
        {

            #region Logging
            if (null != log)
            {
                log.Verbose($"HandlesEventTypeByName({eventTypeFullName})",
                    nameof(Query_Summary_Projection));
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


            return false;
        }


        public  void HandleEvent(QueryCreated eventHandled)
        {
            #region Logging
            if (null != log)
            {
                log.Verbose($"HandleEvent( QueryCreated )",
                    nameof(Query_Summary_Projection ));
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
                    log.Verbose($"Event Handled {eventHandled.QueryName } id: { eventHandled.QueryIdentifier } logged on {eventHandled.Date_Logged }  ",
                        nameof(Query_Summary_Projection ));
                }
                #endregion
            }
            else
            {
                #region Logging
                if (null != log)
                {
                    log.Warning($"HandleEvent( QueryCreated ) - parameter was null",
                        nameof(Query_Summary_Projection ));
                }
                #endregion
            }
        }

        public void HandleEvent(QueryParameterValueSet eventHandled)
        {

            #region Logging
            if (null != log)
            {
                log.Verbose($"HandleEvent( ParameterValueSet )",
                    nameof(Query_Summary_Projection));
            }
            #endregion

            if (null != eventHandled)
            {
                // add or update the parameter value
                string parameterName = @"Parameter." + eventHandled.Name;
                #region Logging
                if (null != log)
                {
                    log.Verbose($" Parameter set {parameterName}",
                        nameof(Query_Summary_Projection));
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
                        log.Verbose($" {eventHandled.Name} set to {eventHandled.Value}  ",
                            nameof(Query_Summary_Projection));
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
                    log.Warning($"HandleEvent( ParameterValueSet ) - parameter was null",
                        nameof(Query_Summary_Projection ));
                }
                #endregion
            }
        }

        public  void HandleEvent(QueryParameterValidationErrorOccured eventHandled)
        {
            #region Logging
            if (null != log)
            {
                log.Verbose($"HandleEvent( QueryParameterValidationErrorOccured )",
                    nameof(Query_Summary_Projection));
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
                    log.Verbose($"Parameter has an invalid value {eventHandled.Name  } Fatal: { eventHandled.Fatal  } Message: {eventHandled.Message  }  ",
                        nameof(Query_Summary_Projection));
                }
                #endregion
            }
            else
            {
                #region Logging
                if (null != log)
                {
                    log.Warning($"HandleEvent( QueryParameterValidationErrorOccured ) - parameter was null",
                        nameof(Query_Summary_Projection));
                }
                #endregion
            }
        }



        public Query_Summary_Projection(TraceWriter logIn = null)
        {
            if (null!= logIn )
            {
                log = logIn;
            }
        }
    }
}
