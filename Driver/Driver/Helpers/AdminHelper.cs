using System.Linq;

namespace Driver.Helpers
{
    public static class AdminHelper
    {
        // List of usernames that have admin privileges
        private static readonly string[] AdminUsernames = { "admin", "Admin", "ADMIN" };

        /// <summary>
        /// Checks if a username has admin privileges
        /// </summary>
        public static bool IsAdmin(string username)
        {
            if (string.IsNullOrEmpty(username))
                return false;

            return AdminUsernames.Contains(username);
        }

        /// <summary>
        /// Adds a new admin username to the list
        /// You can modify this to read from a config file or database
        /// </summary>
        public static void AddAdminUsername(string username)
        {
            // For now, this is static. 
            // In production, you'd want to store this in a database or config file
        }
    }
}