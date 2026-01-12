namespace SoitMed.Common
{
    /// <summary>
    /// Centralized cache key definitions for consistent caching across the application
    /// </summary>
    public static class CacheKeys
    {
        // Cache key prefixes
        private const string Prefix = "SoitMed:";

        // User-related cache keys
        public static class Users
        {
            public static string ById(string userId) => $"{Prefix}User:Id:{userId}";
            public static string ByUsername(string username) => $"{Prefix}User:Username:{username}";
            public static string ByRole(string role) => $"{Prefix}User:Role:{role}";
            public static string All => $"{Prefix}User:All";
            public static string Statistics => $"{Prefix}User:Statistics";
            public static string Counts => $"{Prefix}User:Counts";
        }

        // Reference data cache keys (long expiration)
        public static class Reference
        {
            public static string Departments => $"{Prefix}Reference:Departments";
            public static string Roles => $"{Prefix}Reference:Roles";
            public static string Hospitals => $"{Prefix}Reference:Hospitals";
            public static string HospitalById(string id) => $"{Prefix}Reference:Hospital:{id}";
            public static string Governorates => $"{Prefix}Reference:Governorates";
            public static string GovernorateById(int id) => $"{Prefix}Reference:Governorate:{id}";
        }

        // Product catalog cache keys
        public static class Products
        {
            public static string All => $"{Prefix}Product:All";
            public static string ById(string id) => $"{Prefix}Product:Id:{id}";
            public static string ByCategory(string categoryId) => $"{Prefix}Product:Category:{categoryId}";
            public static string Categories => $"{Prefix}Product:Categories";
            public static string CategoryById(string id) => $"{Prefix}Product:Category:Id:{id}";
            public static string InStock => $"{Prefix}Product:InStock";
        }

        // Client-related cache keys
        public static class Clients
        {
            public static string All => $"{Prefix}Client:All";
            public static string ById(long id) => $"{Prefix}Client:Id:{id}";
            public static string BySalesman(string salesmanId) => $"{Prefix}Client:Salesman:{salesmanId}";
            public static string Analytics(long clientId) => $"{Prefix}Client:Analytics:{clientId}";
        }

        // Offer-related cache keys
        public static class Offers
        {
            public static string BySalesman(string salesmanId) => $"{Prefix}Offer:Salesman:{salesmanId}";
            public static string ByClient(long clientId) => $"{Prefix}Offer:Client:{clientId}";
            public static string ById(long id) => $"{Prefix}Offer:Id:{id}";
            public static string Statistics => $"{Prefix}Offer:Statistics";
            public static string PendingApproval => $"{Prefix}Offer:PendingApproval";
        }

        // Deal-related cache keys
        public static class Deals
        {
            public static string All => $"{Prefix}Deal:All";
            public static string ById(long id) => $"{Prefix}Deal:Id:{id}";
            public static string BySalesman(string salesmanId) => $"{Prefix}Deal:Salesman:{salesmanId}";
            public static string Statistics => $"{Prefix}Deal:Statistics";
        }

        // Maintenance-related cache keys
        public static class Maintenance
        {
            public static string Requests => $"{Prefix}Maintenance:Requests";
            public static string RequestById(int id) => $"{Prefix}Maintenance:Request:{id}";
            public static string PendingRequests => $"{Prefix}Maintenance:Requests:Pending";
            public static string ByEngineer(string engineerId) => $"{Prefix}Maintenance:Engineer:{engineerId}";
            public static string ByCustomer(string customerId) => $"{Prefix}Maintenance:Customer:{customerId}";
        }

        // Dashboard and statistics cache keys
        public static class Dashboard
        {
            public static string SalesManStats(string salesmanId) => $"{Prefix}Dashboard:SalesMan:{salesmanId}";
            public static string SalesManagerStats => $"{Prefix}Dashboard:SalesManager";
            public static string MaintenanceStats => $"{Prefix}Dashboard:Maintenance";
            public static string AdminStats => $"{Prefix}Dashboard:Admin";
        }

        // Notification cache keys (short expiration)
        public static class Notifications
        {
            public static string ByUser(string userId) => $"{Prefix}Notification:User:{userId}";
            public static string UnreadCount(string userId) => $"{Prefix}Notification:UnreadCount:{userId}";
        }

        // Weekly plan cache keys
        public static class WeeklyPlans
        {
            public static string BySalesman(string salesmanId, DateTime weekStart) => 
                $"{Prefix}WeeklyPlan:Salesman:{salesmanId}:Week:{weekStart:yyyy-MM-dd}";
            public static string ById(long id) => $"{Prefix}WeeklyPlan:Id:{id}";
            public static string TasksById(long planId) => $"{Prefix}WeeklyPlan:Tasks:{planId}";
        }

        // Equipment cache keys
        public static class Equipment
        {
            public static string All => $"{Prefix}Equipment:All";
            public static string ById(int id) => $"{Prefix}Equipment:Id:{id}";
            public static string ByHospital(string hospitalId) => $"{Prefix}Equipment:Hospital:{hospitalId}";
            public static string ByQRCode(string qrCode) => $"{Prefix}Equipment:QRCode:{qrCode}";
        }

        // Chat cache keys (very short expiration)
        public static class Chat
        {
            public static string ConversationsByUser(string userId) => $"{Prefix}Chat:Conversations:{userId}";
            public static string MessagesByConversation(long conversationId) => $"{Prefix}Chat:Messages:{conversationId}";
            public static string RecentMessages(long conversationId) => $"{Prefix}Chat:Recent:{conversationId}";
        }

        // Helper methods for cache invalidation patterns
        public static class Patterns
        {
            public static string AllUsers => $"{Prefix}User:*";
            public static string AllProducts => $"{Prefix}Product:*";
            public static string AllClients => $"{Prefix}Client:*";
            public static string AllOffers => $"{Prefix}Offer:*";
            public static string AllDeals => $"{Prefix}Deal:*";
            public static string AllMaintenance => $"{Prefix}Maintenance:*";
            public static string AllNotifications(string userId) => $"{Prefix}Notification:User:{userId}*";
        }
    }
}

