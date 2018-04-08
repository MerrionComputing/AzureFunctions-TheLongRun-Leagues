using System;
using System.Runtime.Serialization;
using CQRSAzure.EventSourcing;

namespace TheLongRun.Common.Events.Command
{
    /// <summary>
    /// A new command has been created
    /// </summary>
    [Serializable()]
    [CQRSAzure.EventSourcing.DomainNameAttribute("Command" )]
    [CQRSAzure.EventSourcing.Category("Command")]
    [CQRSAzure.EventSourcing.EventAsOfDateAttribute("Date_Logged")]
    public class CommandCreated
        : IEvent 
    {

        private readonly string _commandName;
        public string CommandName => _commandName;

        /// <summary>
        /// The date/time the command was logged by the system
        /// </summary>
        private readonly DateTime _Date_Logged;
        public DateTime Date_Logged => _Date_Logged;

        private readonly Guid _commandIdentifier;
        public Guid CommandIdentifier => _commandIdentifier;

        public CommandCreated(string commandNameIn,
            Guid commandIdentifierIn)
        {
            _Date_Logged = DateTime.UtcNow;
            _commandName = commandNameIn;
            _commandIdentifier = commandIdentifierIn;
        }

        public CommandCreated(SerializationInfo info, StreamingContext context)
        {
            _Date_Logged = info.GetDateTime(nameof(Date_Logged) );
            _commandIdentifier = (Guid)info.GetValue(nameof(CommandIdentifier), typeof(Guid));
            _commandName = info.GetString(nameof(CommandName));
        }

        

        /// <summary>
        /// Populates a SerializationInfo with the data needed to serialize this event instance
        /// </summary>
        /// <remarks>
        /// The version number is also to be saved
        /// </remarks>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Date_Logged), _Date_Logged );
            info.AddValue(nameof(CommandIdentifier), _commandIdentifier);
            info.AddValue(nameof(CommandName), _commandName);
        }
    }
}
