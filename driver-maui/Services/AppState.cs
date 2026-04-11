namespace driver_maui.Services
{
    public static class AppState
    {
        public static string Username { get; set; } = "";
        public static int UserId { get; set; }
        public static string Role { get; set; } = ""; // "Student" or "Teacher"
        public static bool IsAdmin { get; set; }

        public static void Clear()
        {
            Username = "";
            UserId = 0;
            Role = "";
            IsAdmin = false;
        }
    }
}
