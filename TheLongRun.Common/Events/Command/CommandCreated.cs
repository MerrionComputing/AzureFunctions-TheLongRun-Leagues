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


        public string CommandName { get; set; }

        /// <summary>
        /// The date/time the command was logged by the system
        /// </summary>
        public DateTime Date_Logged { get; set; }


        public Guid CommandIdentifier { get; set; }

        public CommandCreated(string commandNameIn,
            Guid commandIdentifierIn)
        {
            Date_Logged = DateTime.UtcNow;
            CommandName = commandNameIn;
            CommandIdentifier = commandIdentifierIn;
        }

        public CommandCreated(SerializationInfo info, StreamingContext context)
        {
            Date_Logged = info.GetDateTime(nameof(Date_Logged) );
            CommandIdentifier = (Guid)info.GetValue(nameof(CommandIdentifier), typeof(Guid));
            CommandName = info.GetString(nameof(CommandName));
        }

        

        /// <summary>
        /// Populates a SerializationInfo with the data needed to serialize this event instance
        /// </summary>
        /// <remarks>
        /// The version number is also to be saved
        /// </remarks>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Date_Logged), Date_Logged );
            info.AddValue(nameof(CommandIdentifier), CommandIdentifier);
            info.AddValue(nameof(CommandName), CommandName);
        }
    }
}
