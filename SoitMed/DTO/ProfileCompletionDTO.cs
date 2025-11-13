namespace SoitMed.DTO
{
    /// <summary>
    /// DTO for profile completion status
    /// </summary>
    public class ProfileCompletionDTO
    {
        /// <summary>
        /// Completion percentage (0-100)
        /// </summary>
        public int Progress { get; set; }

        /// <summary>
        /// Number of completed profile fields
        /// </summary>
        public int CompletedSteps { get; set; }

        /// <summary>
        /// Total number of profile fields to complete
        /// </summary>
        public int TotalSteps { get; set; }

        /// <summary>
        /// Number of remaining incomplete fields
        /// </summary>
        public int RemainingSteps { get; set; }

        /// <summary>
        /// List of fields that are completed
        /// </summary>
        public List<string> CompletedFields { get; set; } = new List<string>();

        /// <summary>
        /// List of fields that are missing/incomplete
        /// </summary>
        public List<string> MissingFields { get; set; } = new List<string>();
    }
}

