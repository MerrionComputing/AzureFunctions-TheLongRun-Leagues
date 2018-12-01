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
    /// A projection to get the state of the classifiers running for any given query
    /// </summary>
    public class Query_Classifications_Projection
        : CQRSAzure.EventSourcing.ProjectionBaseUntyped,
        CQRSAzure.EventSourcing.IHandleEvent<ClassificationRequested>,
        CQRSAzure.EventSourcing.IHandleEvent<ClassificationRunStarted>,
        CQRSAzure.EventSourcing.IHandleEvent<ClassificationResultReturned>,
        IProjectionUntyped
    {


        #region Private members
        private ILogger log = null;
        private List<string> keysToBeClassified = new List<string>();
        private List<string> keysBeingClassified = new List<string>();
        private Dictionary<string, UInt32> keysIncluded = new Dictionary<string, uint>();
        private Dictionary<string, UInt32> keysExcluded = new Dictionary<string, uint>();
        #endregion

        /// <summary>
        /// It does not make sense for this projection to support snapshots 
        /// </summary>
        public override bool SupportsSnapshots => false ;

        public override void HandleEvent<TEvent>(TEvent eventToHandle)
        {

            #region Logging
            if (null != log)
            {
                log.LogDebug($"HandleEvent<{ typeof(TEvent).FullName  }>())",
                    nameof(Query_Classifications_Projection));
            }
            #endregion

            if (eventToHandle.GetType() == typeof(ClassificationRequested ))
            {
                HandleEvent(eventToHandle as ClassificationRequested);
            }

            if (eventToHandle.GetType() == typeof(ClassificationRunStarted))
            {
                HandleEvent(eventToHandle as ClassificationRunStarted);
            }

            if (eventToHandle.GetType() == typeof(ClassificationResultReturned))
            {
                HandleEvent(eventToHandle as ClassificationResultReturned);
            }

        }

        public  void HandleEvent(ClassificationRequested eventHandled)
        {
            if (null != eventHandled)
            {
                #region Logging
                if (null != log)
                {
                    log.LogDebug($"Query classification requested  {eventHandled.ClassificationTypeName   } for { eventHandled.DomainName }.{eventHandled.AggregateType}.{eventHandled.AggregateInstanceKey } as at {eventHandled.AsOfDate} ",
                        nameof(Query_Classifications_Projection));
                }
                #endregion
                // add this to the unprocessed list
                keysToBeClassified.Add(eventHandled.AggregateInstanceKey);
            }
        }

        public void HandleEvent(ClassificationRunStarted eventHandled)
        {
            if (null != eventHandled)
            {
                #region Logging
                if (null != log)
                {
                    log.LogDebug($"Query classification run started  {eventHandled.ClassificationTypeName  } for { eventHandled.DomainName }.{eventHandled.AggregateType}.{eventHandled.AggregateInstanceKey } as at {eventHandled.AsOfDate} ",
                        nameof(Query_Classifications_Projection));
                }
                #endregion

                // remove this from this to the unprocessed list
                keysToBeClassified.Remove(eventHandled.AggregateInstanceKey);

                // add it to the in-flight list
                keysBeingClassified.Add(eventHandled.AggregateInstanceKey);
            }
        }

        public void HandleEvent(ClassificationResultReturned eventHandled)
        {
            if (null != eventHandled)
            {
                #region Logging
                if (null != log)
                {
                    log.LogDebug($"Query classification results returned for  {eventHandled.ClassificationTypeName  } for { eventHandled.DomainName }.{eventHandled.AggregateType}.{eventHandled.AggregateInstanceKey } as at {eventHandled.AsOfDate} ",
                        nameof(Query_Classifications_Projection));
                }
                #endregion

                // remove this from this to the unprocessed list - in case it never got into the in-flight set
                keysToBeClassified.Remove(eventHandled.AggregateInstanceKey);

                // remove this from the in-flight list
                keysBeingClassified.Remove (eventHandled.AggregateInstanceKey);

                if (eventHandled.IsInGroup)
                {
                    keysExcluded.Remove(eventHandled.AggregateInstanceKey);
                    keysIncluded.Add(eventHandled.AggregateInstanceKey, eventHandled.AsOfSequenceNumber); 
                }
                else
                {
                    keysIncluded.Remove(eventHandled.AggregateInstanceKey);
                    keysExcluded.Add(eventHandled.AggregateInstanceKey, eventHandled.AsOfSequenceNumber);
                }
            }
        }

        public override void HandleEventJSon(string eventFullName, JObject eventToHandle)
        {
            #region Logging
            if (null != log)
            {
                log.LogDebug($"HandleEventJSon({eventFullName})",
                    nameof(Query_Classifications_Projection));
            }
            #endregion


            if (eventFullName == typeof(ClassificationRequested).FullName)
            {
                HandleEvent<ClassificationRequested>(eventToHandle.ToObject<ClassificationRequested>());
            }

            if (eventFullName == typeof(ClassificationRunStarted ).FullName)
            {
                HandleEvent<ClassificationRunStarted>(eventToHandle.ToObject<ClassificationRunStarted>());
            }

            if (eventFullName == typeof(ClassificationResultReturned).FullName)
            {
                HandleEvent<ClassificationResultReturned>(eventToHandle.ToObject<ClassificationResultReturned>());
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
                log.LogDebug($"HandlesEventTypeByName({eventTypeFullName})",
                    nameof(Query_Projections_Projection));
            }
            #endregion


            if (eventTypeFullName == typeof(ClassificationRequested).FullName)
            {
                return true;
            }

            if (eventTypeFullName == typeof(ClassificationResultReturned).FullName)
            {
                return true;
            }

            if (eventTypeFullName == typeof(ClassificationResultReturned).FullName)
            {
                return true;
            }

            return false;
        }


        public Query_Classifications_Projection(ILogger logIn = null)
        {
            if (null != logIn)
            {
                log = logIn;
            }
        }
    }
}
