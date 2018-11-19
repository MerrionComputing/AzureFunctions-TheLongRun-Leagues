using System;
using System.Collections.Generic;
using CQRSAzure.EventSourcing;
using CQRSAzure.QueryDefinition;
using Leagues;

namespace Leagues.League.queryDefinition
{
    /// <summary>
    /// Data object returned from the "Get league summary" query
    /// </summary>
    [CQRSAzure.EventSourcing.DomainNameAttribute("Leagues")]
    [CQRSAzure.EventSourcing.Category("Organisation")]
    public class Get_League_Summary_Definition_Return
        : IGet_League_Summary_Return
    {

        /// <summary>
        /// The unique identifier of the query for which this result was returned
        /// </summary>
        private readonly Guid queryId;
        public Guid QueryIdentifier
        {
            get
            {
                return queryId;
            }
        }

        /// <summary>
        /// The unique identifier for the league for which this query data are returned
        /// </summary>
        private readonly string leagueName;
        public string LeagueName
        {
            get
            {
                return leagueName;
            }
        }

        public string Location { get; set; }

        public string Twitter_Handle { get; set;  }

        public DateTime Date_Incorporated { get; set; }

        public Get_League_Summary_Definition_Return(
            Guid queryIdIn,
            string leagueNameIn)
        {
            queryId = queryIdIn;
            leagueName = leagueNameIn;
        }

    }
}
