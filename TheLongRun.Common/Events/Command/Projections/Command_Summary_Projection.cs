using System;
using System.Collections;
using System.Collections.Generic;

using CQRSAzure.EventSourcing;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json.Linq;
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
        CQRSAzure.EventSourcing.IHandleEvent<ProcessingCompleted >,
        IProjectionUntyped
    {


        private List<string> parameterNames = new List<string>();
        private TraceWriter log = null;

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
                    nameof(Command_Summary_Projection));
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

        /// <summary>
        /// The name of the command being executed
        /// </summary>
        public string CommandName
        {
            get
            {
                #region Logging
                if (null != log)
                {
                    log.Verbose($"CommandName - Get ",
                        nameof(Command_Summary_Projection));
                }
                #endregion
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
            /// A multi-step command can be valid but not yet complete so a status of "In progress" would indicate that
            /// </summary>
            InProgress = 3,
            /// <summary>
            /// A command marked as complete
            /// </summary>
            Completed =4
        }

        public CommandState CurrentState
        {
            get
            {
                #region Logging
                if (null != log)
                {
                    log.Verbose($"CurrentState - Get ",
                        nameof(Command_Summary_Projection));
                }
                #endregion
                return base.GetPropertyValue<CommandState>(nameof(CurrentState)); 
            }
        }

        private  IDictionary<string , object > Parameters
        {
            get
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                foreach (string  paramName in parameterNames )
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
            if (ParameterIsSet(parameterName ) )
            {
                return base.GetPropertyValue<TParam>("Parameter." + parameterName);
            }
            else
            {
                return default(TParam);
            }
        }

        /// <summary>
        /// There is no value in storing snapshots for command summaries
        /// </summary>
        public override bool SupportsSnapshots => false ;

        public override void HandleEvent<TEvent>(TEvent eventToHandle)
        {

            #region Logging
            if (null != log)
            {
                log.Verbose($"HandleEvent<{ typeof(TEvent).FullName  }>())",
                    nameof(Command_Summary_Projection));
            }
            #endregion

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

            if (eventToHandle.GetType() == typeof(ProcessingCompleted ))
            {
                HandleEvent(eventToHandle as ProcessingCompleted);
            }

        }

        public  void HandleEvent(CommandCreated eventHandled)
        {
            #region Logging
            if (null != log)
            {
                log.Verbose($"HandleEvent( CommandCreated )",
                    nameof(Command_Summary_Projection));
            }
            #endregion

            if (null != eventHandled)
            {
                // Set the properties from this event
                base.AddOrUpdateValue<string>(nameof(CommandName), 0, eventHandled.CommandName);
                // Set the status as "Created"
                base.AddOrUpdateValue<CommandState>(nameof(CurrentState), 0, CommandState.Created);
                #region Logging
                if (null != log)
                {
                    log.Verbose($"Event Handled {eventHandled.CommandName} id: { eventHandled.CommandIdentifier} logged on {eventHandled.Date_Logged }  ",
                        nameof(Command_Summary_Projection));
                }
                #endregion
            }
            else
            {
                #region Logging
                if (null != log)
                {
                    log.Warning ($"HandleEvent( CommandCreated ) - parameter was null",
                        nameof(Command_Summary_Projection));
                }
                #endregion
            }
        }


        public void HandleEvent(ParameterValueSet eventHandled)
        {

            #region Logging
            if (null != log)
            {
                log.Verbose($"HandleEvent( ParameterValueSet )",
                    nameof(Command_Summary_Projection));
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
                        nameof(Command_Summary_Projection));
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
                            nameof(Command_Summary_Projection));
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
                        nameof(Command_Summary_Projection));
                }
                #endregion
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
            #region Logging
            if (null != log)
            {
                log.Verbose($"HandleEvent( ValidationErrorOccured )",
                    nameof(Command_Summary_Projection));
            }
            #endregion

            if (null != eventHandled )
            {
                // Set the status as "Invalid"
                base.AddOrUpdateValue<CommandState>(nameof(CurrentState), 0, CommandState.Invalid);
            }
            else
            {
                #region Logging
                if (null != log)
                {
                    log.Warning($"HandleEvent( ValidationErrorOccured ) - parameter was null",
                        nameof(Command_Summary_Projection));
                }
                #endregion
            }
        }

        public  void HandleEvent(ValidationSucceeded eventHandled)
        {
            #region Logging
            if (null != log)
            {
                log.Verbose($"HandleEvent( ValidationSucceeded )",
                    nameof(Command_Summary_Projection));
            }
            #endregion

            if (null != eventHandled)
            {
                // Set the status as "Validated"
                base.AddOrUpdateValue<CommandState>(nameof(CurrentState), 0, CommandState.Validated);
            }
            else
            {
                #region Logging
                if (null != log)
                {
                    log.Warning($"HandleEvent( ValidationSucceeded ) - parameter was null",
                        nameof(Command_Summary_Projection));
                }
                #endregion
            }
        }

        public void HandleEvent(ProcessingCompleted  eventHandled)
        {
            #region Logging
            if (null != log)
            {
                log.Verbose($"HandleEvent( ProcessingCompleted )",
                    nameof(Command_Summary_Projection));
            }
            #endregion

            if (null != eventHandled)
            {
                // Set the status as "Validated"
                base.AddOrUpdateValue<CommandState>(nameof(CurrentState), 0, CommandState.Completed);
            }
            else
            {
                #region Logging
                if (null != log)
                {
                    log.Warning($"HandleEvent( ProcessingCompleted ) - parameter was null",
                        nameof(Command_Summary_Projection));
                }
                #endregion
            }
        }

        public override void HandleEventJSon(string eventFullName, JObject eventToHandle)
        {
            #region Logging
            if (null != log)
            {
                log.Verbose($"HandleEventJSon({eventFullName})",
                    nameof(Command_Summary_Projection));
            }
            #endregion

            if (eventFullName == typeof(CommandCreated).FullName)
            {
                HandleEvent<CommandCreated >(eventToHandle.ToObject<CommandCreated>())  ;
            }

            if (eventFullName == typeof(ParameterValueSet).FullName)
            {
                HandleEvent<ParameterValueSet>(eventToHandle.ToObject<ParameterValueSet>());
            }

            if (eventFullName == typeof(ValidationErrorOccured).FullName)
            {
                HandleEvent<ValidationErrorOccured>(eventToHandle.ToObject<ValidationErrorOccured>());
            }

            if (eventFullName == typeof(ValidationSucceeded).FullName)
            {
                HandleEvent<ValidationSucceeded>(eventToHandle.ToObject<ValidationSucceeded>());
            }

            if (eventFullName == typeof(ProcessingCompleted ).FullName)
            {
                HandleEvent<ProcessingCompleted>(eventToHandle.ToObject<ProcessingCompleted>());
            }
        }

        public override bool HandlesEventTypeByName(string eventTypeFullName)
        {

            #region Logging
            if (null != log )
            {
                log.Verbose($"HandlesEventTypeByName({eventTypeFullName})",
                    nameof(Command_Summary_Projection)); 
            }
            #endregion

            if (eventTypeFullName == typeof(CommandCreated).FullName )
            {
                return true;
            }

            if (eventTypeFullName == typeof(ParameterValueSet).FullName )
            {
                return true;
            }

            if (eventTypeFullName == typeof(ValidationErrorOccured).FullName )
            {
                return true;
            }

            if (eventTypeFullName == typeof(ValidationSucceeded).FullName )
            {
                return true;
            }

            if (eventTypeFullName == typeof(ProcessingCompleted ).FullName)
            {
                return true;
            }

            #region Logging
            if (null != log)
            {
                log.Verbose($"{eventTypeFullName} was not handled",
                    nameof(Command_Summary_Projection));
            }
            #endregion

            return false;
        }

        public Command_Summary_Projection(TraceWriter logIn = null)
        {
            if (null != logIn )
            {
                log = logIn;
            }
            // Initialise parameters
            base.CreateProperty<CommandState>(nameof(CurrentState) );
            base.CreateProperty<string>(nameof(CommandName));
            
        }
    }
}
