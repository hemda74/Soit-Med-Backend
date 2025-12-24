namespace SoitMed.Models
{
    /// <summary>
    /// Constants for client classification (A, B, C, D)
    /// </summary>
    public static class ClientClassificationConstants
    {
        public const string A = "A";
        public const string B = "B";
        public const string C = "C";
        public const string D = "D";

        public static readonly string[] AllClassifications = { A, B, C, D };

        public static bool IsValidClassification(string classification)
        {
            return AllClassifications.Contains(classification);
        }
    }
}

