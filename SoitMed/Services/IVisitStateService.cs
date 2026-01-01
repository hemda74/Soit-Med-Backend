using SoitMed.Models.Enums;

namespace SoitMed.Services
{
    /// <summary>
    /// Interface for visit state management service
    /// </summary>
    public interface IVisitStateService
    {
        /// <summary>
        /// Checks if a state transition is valid
        /// </summary>
        bool CanTransition(VisitStatus from, VisitStatus to);

        /// <summary>
        /// Validates a state transition and throws exception if invalid
        /// </summary>
        void ValidateTransition(VisitStatus from, VisitStatus to);

        /// <summary>
        /// Gets all valid next states for a given current state
        /// </summary>
        VisitStatus[] GetValidNextStates(VisitStatus current);

        /// <summary>
        /// Checks if a state is terminal (no further transitions allowed)
        /// </summary>
        bool IsTerminalState(VisitStatus state);

        /// <summary>
        /// Gets the initial state based on user role
        /// </summary>
        VisitStatus GetInitialState(string userRole);
    }
}

