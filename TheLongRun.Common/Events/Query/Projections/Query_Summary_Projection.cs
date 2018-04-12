using System;
using System.Collections;
using System.Collections.Generic;

using CQRSAzure.EventSourcing;
using TheLongRun.Common.Events.Query;

namespace TheLongRun.Common.Events.Query.Projections
{
    /// <summary>
    /// A projection to get the current state of a command based on the events that have occured to it
    /// </summary>
    [CQRSAzure.EventSourcing.DomainNameAttribute("Query")]
    [CQRSAzure.EventSourcing.Category("Query")]
    public class Query_Summary_Projection
    {

    }
}
