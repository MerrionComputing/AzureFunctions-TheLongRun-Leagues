using System;
using System.Runtime.Serialization;
using CQRSAzure.EventSourcing;

namespace TheLongRun.Common.Events.Command
{
    /// <summary>
    /// A new command has been created
    /// </summary>
    [Serializable()]
    [CQRSAzure.EventSourcing.DomainNameAttribute(Constants.Domain_Command )]
    [CQRSAzure.EventSourcing.Category(Constants.Domain_Command)]
    [CQRSAzure.EventSourcing.EventAsOfDateAttribute(nameof (Date_Logged))]
    public class CommandCreated
        : IEvent 
    {

        /// <summary>
        /// The name of the command to be executed
        /// </summary>
        public string CommandName { get; set; }

        /// <summary>
        /// The date/time the command was logged by the system
        /// </summary>
        public DateTime Date_Logged { get; set; }


        public Guid CommandIdentifier { get; set; }

        /// <summary>
        /// The correlation identifier to be used to link the operations of an individual
        /// command together.  
        /// </summary>
        /// <remarks>
        /// This may be set by an external system or command caller and used by it to track
        /// progress
        /// </remarks>
        public string CorrelationIdentifier { get; set; }

        /// <summary>
        /// For commands that rely on authorisation this is the token passed in to test
        /// for the authorisation process
        /// </summary>
        public string AuthorisationToken { get; set; }

        public CommandCreated(string commandNameIn,
            Guid commandIdentifierIn,
            string correlationIdentifierIn = @"",
            string aurhorisationTokenIn = @"")
        {
            Date_Logged = DateTime.UtcNow;
            CommandName = commandNameIn;
            CommandIdentifier = commandIdentifierIn;
            CorrelationIdentifier = correlationIdentifierIn;
            AuthorisationToken = aurhorisationTokenIn;
        }

        /// <summary>
        /// Empty constructor for serialisation
        /// </summary>
        public CommandCreated() { }

        public CommandCreated(SerializationInfo info, StreamingContext context)
        {
            Date_Logged = info.GetDateTime(nameof(Date_Logged) );
            CommandIdentifier = (Guid)info.GetValue(nameof(CommandIdentifier), typeof(Guid));
            CommandName = info.GetString(nameof(CommandName));
            CorrelationIdentifier = info.GetString(nameof(CorrelationIdentifier));
            AuthorisationToken = info.GetString(nameof(AuthorisationToken));
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
            info.AddValue(nameof(CorrelationIdentifier), CorrelationIdentifier);
            info.AddValue(nameof(AuthorisationToken), AuthorisationToken);
        }
    }
}
