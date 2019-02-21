//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using CQRSAzure.EventSourcing;
using Leagues.League.eventDefinition;

namespace Leagues.League.projection
{

    public partial interface ILeague_Summary_Information : 
        IProjectionUntyped
    {
        
        /// <summary>
        /// Current location of the league
        /// </summary>
        string Location
        {
            get;
        }
        
        /// <summary>
        /// The twitter handle used by this league 
        /// </summary>
        /// <remarks>
        /// This can be used to broadcast results or notifications
        /// </remarks>
        string Twitter_Handle
        {
            get;
        }
        
        /// <summary>
        /// The date this league was incorporated
        /// </summary>
        System.DateTime Date_Incorporated
        {
            get;
        }
    }
}
