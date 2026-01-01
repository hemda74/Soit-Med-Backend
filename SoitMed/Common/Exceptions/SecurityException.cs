namespace SoitMed.Common.Exceptions
{
    /// <summary>
    /// Exception thrown when a security violation occurs (e.g., QR code mismatch)
    /// </summary>
    public class SecurityException : Exception
    {
        public string Operation { get; }
        public string? ResourceId { get; }

        public SecurityException(string operation, string? resourceId = null)
            : base($"Security violation in operation '{operation}'. Access denied.")
        {
            Operation = operation;
            ResourceId = resourceId;
        }

        public SecurityException(string operation, string? resourceId, string message)
            : base(message)
        {
            Operation = operation;
            ResourceId = resourceId;
        }
    }
}

