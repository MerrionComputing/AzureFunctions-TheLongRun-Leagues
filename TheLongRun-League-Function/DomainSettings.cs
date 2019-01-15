using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.WebJobs;

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

    }
}
