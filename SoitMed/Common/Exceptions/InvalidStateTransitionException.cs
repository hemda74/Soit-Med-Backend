namespace SoitMed.Common.Exceptions
{
    /// <summary>
    /// Exception thrown when an invalid state transition is attempted
    /// </summary>
    public class InvalidStateTransitionException : Exception
    {
        public string CurrentState { get; }
        public string AttemptedState { get; }
        public string EntityName { get; }

        public InvalidStateTransitionException(string currentState, string attemptedState, string entityName = "Entity")
            : base($"Invalid state transition for {entityName}: Cannot transition from '{currentState}' to '{attemptedState}'")
        {
            CurrentState = currentState;
            AttemptedState = attemptedState;
            EntityName = entityName;
        }

        public InvalidStateTransitionException(string currentState, string attemptedState, string entityName, string message)
            : base(message)
        {
            CurrentState = currentState;
            AttemptedState = attemptedState;
            EntityName = entityName;
        }
    }
}

