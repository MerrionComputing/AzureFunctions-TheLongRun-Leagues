using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.WebJobs;

using TheLongRun.Common.Model;

namespace TheLongRunLeaguesFunction
{
    /// <summary>
    /// Common settings and defaults applicable for the whole domain
    /// </summary>
    public static class DomainSettings
    {

        /// <summary>
        /// Retry settings to use when retryng a query-based acttivity
        /// </summary>
        public static RetryOptions QueryRetryOptions()
        {
            return new RetryOptions(new TimeSpan(0, 1, 0), 2);
        }

        /// <summary>
        /// Retry settings to use when retryng a command-based acttivity
        /// </summary>
        public static RetryOptions CommandRetryOptions()
        {
            return new RetryOptions(new TimeSpan(0, 0, 10), 2);
        }

        /// <summary>
        /// A hard-coded domain model for faster loading than using reflection
        /// </summary>
        public static Domains GetDomainModel()
        {
            return new Domains()
                .Add(new Domain("Leagues")
                  .Add(new EntityType("League", domainParentName: "Leagues", connectionStringName: "LeagueStorageConnectionString")
                    .Add(new ProjectionDefinition("Command Status", "GetCommandStatusInformationProjection"))
                    .Add(new IdentifierGroupDefinition("All Leagues", "GetAllLeaguesIdentifierGroup")))
                  .Add(new CommandDefinition("Create League", "OnCreateLeagueCommand"))
                  .Add(new CommandDefinition("Set Email Address", "OnSetLeagueEmailAddressCommandHandler"))
                  .Add(new QueryDefinition("Get League Summary", "OnGetLeagueSummaryQueryHandler"))
                ); 
        }

    }
}
