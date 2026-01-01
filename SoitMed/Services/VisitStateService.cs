using SoitMed.Common.Exceptions;
using SoitMed.Models.Enums;

namespace SoitMed.Services
{
    /// <summary>
    /// Service for managing and validating visit state transitions
    /// Implements state machine pattern to enforce valid status changes
    /// </summary>
    public class VisitStateService : IVisitStateService
    {
        // Define valid state transitions
        // Key: Current state, Value: List of valid next states
        private static readonly Dictionary<VisitStatus, List<VisitStatus>> ValidTransitions = new()
        {
            // PendingApproval can only go to Scheduled (approved) or Cancelled (rejected)
            { VisitStatus.PendingApproval, new List<VisitStatus> { VisitStatus.Scheduled, VisitStatus.Cancelled } },
            
            // Scheduled can go to InProgress (when engineer starts), Rescheduled, or Cancelled
            { VisitStatus.Scheduled, new List<VisitStatus> { VisitStatus.InProgress, VisitStatus.Rescheduled, VisitStatus.Cancelled } },
            
            // InProgress can go to NeedsSpareParts, Completed, or Rescheduled
            { VisitStatus.InProgress, new List<VisitStatus> { VisitStatus.NeedsSpareParts, VisitStatus.Completed, VisitStatus.Rescheduled } },
            
            // NeedsSpareParts can go back to InProgress (after parts arrive) or Completed (if parts not needed)
            { VisitStatus.NeedsSpareParts, new List<VisitStatus> { VisitStatus.InProgress, VisitStatus.Completed } },
            
            // Completed can go to Rescheduled (if follow-up needed)
            { VisitStatus.Completed, new List<VisitStatus> { VisitStatus.Rescheduled } },
            
            // Rescheduled can go to Scheduled (new visit scheduled)
            { VisitStatus.Rescheduled, new List<VisitStatus> { VisitStatus.Scheduled } },
            
            // Cancelled is a terminal state - no transitions allowed
            { VisitStatus.Cancelled, new List<VisitStatus>() }
        };

        /// <summary>
        /// Checks if a state transition is valid
        /// </summary>
        public bool CanTransition(VisitStatus from, VisitStatus to)
        {
            // Same state is always valid (no-op)
            if (from == to)
                return true;

            // Check if transition is in the valid transitions dictionary
            if (!ValidTransitions.ContainsKey(from))
                return false;

            return ValidTransitions[from].Contains(to);
        }

        /// <summary>
        /// Validates a state transition and throws exception if invalid
        /// </summary>
        public void ValidateTransition(VisitStatus from, VisitStatus to)
        {
            if (!CanTransition(from, to))
            {
                throw new InvalidStateTransitionException(
                    from.ToString(),
                    to.ToString(),
                    "MaintenanceVisit",
                    $"Cannot transition visit from '{from}' to '{to}'. Valid next states: {string.Join(", ", GetValidNextStates(from))}"
                );
            }
        }

        /// <summary>
        /// Gets all valid next states for a given current state
        /// </summary>
        public VisitStatus[] GetValidNextStates(VisitStatus current)
        {
            if (!ValidTransitions.ContainsKey(current))
                return Array.Empty<VisitStatus>();

            return ValidTransitions[current].ToArray();
        }

        /// <summary>
        /// Checks if a state is terminal (no further transitions allowed)
        /// </summary>
        public bool IsTerminalState(VisitStatus state)
        {
            if (!ValidTransitions.ContainsKey(state))
                return false;

            return ValidTransitions[state].Count == 0;
        }

        /// <summary>
        /// Gets the initial state based on user role
        /// </summary>
        public VisitStatus GetInitialState(string userRole)
        {
            // SalesSupport creates visits that need approval
            if (userRole == "SalesSupport")
                return VisitStatus.PendingApproval;

            // MaintenanceSupport and Managers can create scheduled visits directly
            if (userRole == "MaintenanceSupport" || userRole == "MaintenanceManager" || userRole == "SuperAdmin")
                return VisitStatus.Scheduled;

            // Default to PendingApproval for safety
            return VisitStatus.PendingApproval;
        }
    }
}

