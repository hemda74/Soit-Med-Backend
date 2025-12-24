namespace SoitMed.Models
{
    /// <summary>
    /// Constants for client status in the context of weekly plan tasks
    /// </summary>
    public static class ClientStatusConstants
    {
        public const string Old = "Old";
        public const string New = "New";

        public static readonly string[] AllStatuses = { Old, New };

        public static bool IsValidStatus(string status)
        {
            return AllStatuses.Contains(status);
        }
    }
}

