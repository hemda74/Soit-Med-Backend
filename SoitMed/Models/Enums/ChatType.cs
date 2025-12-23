namespace SoitMed.Models.Enums
{
    /// <summary>
    /// Chat conversation types that determine which support role handles the conversation
    /// </summary>
    public enum ChatType
    {
        /// <summary>
        /// App problems, account issues, or complaints - handled by Admin role
        /// </summary>
        Support = 0,

        /// <summary>
        /// Sales-related inquiries - handled by SalesSupport role
        /// </summary>
        Sales = 1,

        /// <summary>
        /// Maintenance-related inquiries - handled by MaintenanceSupport role
        /// </summary>
        Maintenance = 2
    }
}

