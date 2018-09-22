using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheLongRun.Common.Orchestration
{
    public interface IIdentifierGroupRunner
    {

        /// <summary>
        /// Get the members of the specified identifier group
        /// </summary>
        /// <param name="groupName">
        /// The domain qualified name of the identifier group to get the members of
        /// </param>
        /// <param name="instanceId">
        /// The global identifier of the projection instance if we are re-using an existing one (to get an up-to-date value for instance)
        /// </param>
        /// <param name="asOfDate">
        /// The date up until which the identifier group membership should run
        /// </param>
        /// <remarks>
        /// If the as-of-date is not supplied, the classifier is run to the latest event
        /// </remarks>
        Task<IEnumerable<IIdentifierGroupMemberResponse>> GetIdentifierGroupMembersAsync(string groupName, 
            string instanceId, 
            DateTime? asOfDate = null);
    }


    /// <summary>
    /// The response indicating that the given member IS in the identifier group as at the requested time
    /// </summary>
    public interface IIdentifierGroupMemberResponse:
        IAsOfDateOrchestrationResponse 
    {

    }
}
