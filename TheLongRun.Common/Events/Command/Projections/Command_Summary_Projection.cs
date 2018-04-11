using System;
using System.Collections;
using System.Collections.Generic;

using CQRSAzure.EventSourcing;
using TheLongRun.Common.Events.Command;

namespace TheLongRun.Common.Events.Command.Projections
{
    /// <summary>
    /// A projection to get the current state of a command based on the events that have occured to it
    /// </summary>
    [CQRSAzure.EventSourcing.DomainNameAttribute("Command")]
    [CQRSAzure.EventSourcing.Category("Command")]
    public class Command_Summary_Projection :
        CQRSAzure.EventSourcing.ProjectionBaseUntyped,
        CQRSAzure.EventSourcing.IHandleEvent<CommandCreated>,
        CQRSAzure.EventSourcing.IHandleEvent<ParameterValueSet >,
        CQRSAzure.EventSourcing.IHandleEvent<ValidationErrorOccured >,
        CQRSAzure.EventSourcing.IHandleEvent<ValidationSucceeded >,
        IProjectionUntyped
    {


        private List<string> parameterNames = new List<string>();

        /// <summary>
        /// Is there a value set for the named parameter?
        /// </summary>
        /// <param name="parameterName">
        /// The name of the parameter
        /// </param>
        public bool ParameterIsSet(string parameterName)
        {
            if (parameterName.Contains(parameterName))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// The name of the command being executed
        /// </summary>
        public string CommandName
        {
            get
            {
                return base.GetPropertyValue<string>(nameof(CommandName));
            }
        }

        /// <summary>
        /// The different states a command can be in - to prevent them being processed
        /// in an invalid state
        /// </summary>
        public enum CommandState
        {
            /// <summary>
            /// A new command that has just been created
            /// </summary>
            Created = 0,
            /// <summary>
            /// A command that has been validated and can proceed
            /// </summary>
            Validated = 1,
            /// <summary>
            /// A command marked as invalid
            /// </summary>
            Invalid = 2,
            /// <summary>
            /// A command marked as complete
            /// </summary>
            Completed =3
        }

        public CommandState CurrentState
        {
            get
            {
                return base.GetPropertyValue<CommandState>(nameof(CurrentState)); 
            }
        }

        public IDictionary<string , object > Parameters
        {
            get
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                foreach (string  paramName in parameterNames )
                {
                    parameters.Add(paramName, base.GetPropertyValue<object>("Parameter." + paramName));
                }
                return parameters;
            }
        }


        /// <summary>
        /// There is no value in storing snapshots for command summaries
        /// </summary>
        public override bool SupportsSnapshots => false ;

        public override void HandleEvent<TEvent>(TEvent eventToHandle)
        {
            if (eventToHandle.GetType() == typeof(CommandCreated))
            {
                HandleEvent(eventToHandle as CommandCreated);
            }

            if (eventToHandle.GetType() == typeof(ParameterValueSet ))
            {
                HandleEvent(eventToHandle as ParameterValueSet);
            }

            if (eventToHandle.GetType() == typeof(ValidationSucceeded ))
            {
                HandleEvent(eventToHandle as ValidationSucceeded );
            }

            if (eventToHandle.GetType() == typeof(ValidationErrorOccured ))
            {
                HandleEvent(eventToHandle as ValidationErrorOccured);
            }

        }

        public  void HandleEvent(CommandCreated eventHandled)
        {
            if (null != eventHandled)
            {
                // Set the properties from this event
                base.AddOrUpdateValue<string>(nameof(CommandName), 0, eventHandled.CommandName);
                // Set the status as "Created"
                base.AddOrUpdateValue<CommandState>(nameof(CommandState), 0, CommandState.Created); 
            }
        }


        public void HandleEvent(ParameterValueSet eventHandled)
        {
            if (null != eventHandled)
            {
                // add or update the parameter value
                string parameterName = @"Parameter." + eventHandled.Name;
                base.AddOrUpdateValue(parameterName, 0, eventHandled.Value);
                if (!parameterNames.Contains(eventHandled.Name ) )
                {
                    parameterNames.Add(eventHandled.Name);
                }
            }
        }

        public override bool HandlesEventType(Type eventType)
        {
            if (eventType == typeof(CommandCreated ))
            {
                return true;
            }

            if (eventType == typeof(ParameterValueSet ))
            {
                return true;
            }

            if (eventType == typeof(ValidationErrorOccured ))
            {
                return true;
            }

            if (eventType == typeof(ValidationSucceeded ))
            {
                return true;
            }

            return false;
        }

        public  void HandleEvent(ValidationErrorOccured eventHandled)
        {
            if (null != eventHandled )
            {
                // Set the status as "Invalid"
                base.AddOrUpdateValue<CommandState>(nameof(CommandState), 0, CommandState.Invalid);
            }
        }

        public  void HandleEvent(ValidationSucceeded eventHandled)
        {
            if (null != eventHandled)
            {
                // Set the status as "Validated"
                base.AddOrUpdateValue<CommandState>(nameof(CommandState), 0, CommandState.Validated);
            }
        }
    }
}
