using System;
using System.Collections;
using System.Collections.Generic;

using CQRSAzure.EventSourcing;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json.Linq;
using TheLongRun.Common.Events.IdentifierGroup;

namespace TheLongRun.Common.Events.IdentifierGroup.Projections
{
    /// <summary>
    /// Projection to list the members of an identifier group
    /// </summary>
    [CQRSAzure.EventSourcing.DomainNameAttribute("Identifier Group")]
    [CQRSAzure.EventSourcing.Category("Group")]
    public class Membership_Projection:
        CQRSAzure.EventSourcing.ProjectionBaseUntyped,
        CQRSAzure.EventSourcing.IHandleEvent<MemberExcluded >,
        CQRSAzure.EventSourcing.IHandleEvent<MemberIncluded >,
        IProjectionUntyped
    {

        #region private members
        private List<string> members = new List<string>();
        private TraceWriter log = null;
        #endregion

        /// <summary>
        /// This projection does not currently support snapshots 
        /// </summary>
        public override bool SupportsSnapshots => false ;

        public override void HandleEvent<TEvent>(TEvent eventToHandle)
        {
            #region Logging
            if (null != log)
            {
                log.Verbose($"HandleEvent<{ typeof(TEvent).FullName  }>())",
                    nameof(Membership_Projection));
            }
            #endregion

            if (eventToHandle.GetType() == typeof(MemberExcluded ))
            {
                HandleEvent(eventToHandle as MemberExcluded);
            }

            if (eventToHandle.GetType() == typeof(MemberIncluded ))
            {
                HandleEvent(eventToHandle as MemberIncluded);
            }
        }

        public override void HandleEventJSon(string eventFullName, JObject eventToHandle)
        {
            #region Logging
            if (null != log)
            {
                log.Verbose($"HandleEventJSon({eventFullName})",
                    nameof(Membership_Projection));
            }
            #endregion

            if (eventFullName == typeof(MemberIncluded ).FullName)
            {
                HandleEvent<MemberIncluded>(eventToHandle.ToObject<MemberIncluded>());
            }

            if (eventFullName == typeof(MemberExcluded ).FullName)
            {
                HandleEvent<MemberExcluded>(eventToHandle.ToObject<MemberExcluded>());
            }
        }

        public override bool HandlesEventType(Type eventType)
        {

            if (eventType == typeof(MemberIncluded ))
            {
                return true;
            }

            if (eventType == typeof(MemberExcluded ))
            {
                return true;
            }

            return false;
        }

        public override bool HandlesEventTypeByName(string eventTypeFullName)
        {

            #region Logging
            if (null != log)
            {
                log.Verbose($"HandlesEventTypeByName({eventTypeFullName})",
                    nameof(Membership_Projection));
            }
            #endregion

            if (eventTypeFullName == typeof(MemberExcluded ).FullName)
            {
                return true;
            }

            if (eventTypeFullName == typeof(MemberIncluded ).FullName)
            {
                return true;
            }


            return false;
        }

        /// <summary>
        /// A member was excluded from the identifier group
        /// </summary>
        /// <param name="eventHandled">
        /// Details of the member that was excluded
        /// </param>
        public  void HandleEvent(MemberExcluded eventHandled)
        {
            #region Logging
            if (null != log)
            {
                log.Verbose($"HandleEvent( MemberExcluded ) : {eventHandled.AggregateInstanceKey} ",
                    nameof(Membership_Projection));
            }
            #endregion
            members.Remove (eventHandled.AggregateInstanceKey);
        }

        /// <summary>
        /// A member was included in the identifier group
        /// </summary>
        /// <param name="eventHandled">
        /// Details of the event for the member being included
        /// </param>
        public  void HandleEvent(MemberIncluded eventHandled)
        {
            #region Logging
            if (null != log)
            {
                log.Verbose($"HandleEvent( MemberIncluded ) : {eventHandled.AggregateInstanceKey} ",
                    nameof(Membership_Projection));
            }
            #endregion
            members.Add(eventHandled.AggregateInstanceKey);
        }


        /// <summary>
        /// Create a new instance of the group membership projection
        /// </summary>
        /// <param name="logIn">
        /// Trace output writer (optional)
        /// </param>
        public Membership_Projection(TraceWriter logIn = null)
        {
            if (null != logIn)
            {
                log = logIn;
            }
        }

    }
}
