using System;
using System.Collections.Generic;
using CQRSAzure.EventSourcing;
using CQRSAzure.QueryDefinition;
using Leagues;
using Newtonsoft.Json.Linq;

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
        public Guid QueryIdentifier { get; set; }


        /// <summary>
        /// The unique identifier for the league for which this query data are returned
        /// </summary>
        public string LeagueName { get; set; }


        public string Location { get; set; }

        public string Twitter_Handle { get; set;  }

        public DateTime Date_Incorporated { get; set; }

        public Get_League_Summary_Definition_Return(
            Guid queryIdIn,
            string leagueNameIn)
        {
            QueryIdentifier  = queryIdIn;
            LeagueName  = leagueNameIn;
        }

        /// <summary>
        /// Empty constructor for serialisation
        /// </summary>
        public Get_League_Summary_Definition_Return()
        { }

    }
}
