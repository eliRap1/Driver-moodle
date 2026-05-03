namespace driver_maui.Services
{
    public static class AppState
    {
        public static string Username { get; set; } = "";
        public static int UserId { get; set; }
        public static string Role { get; set; } = ""; // "Student" or "Teacher"
        public static bool IsAdmin { get; set; }

        public static bool IsTeacher => Role == "Teacher";
        public static bool IsStudent => Role == "Student";
        public static bool IsLoggedIn => UserId > 0 && !string.IsNullOrEmpty(Role);

        /// <summary>
        /// Page-side guard: redirects to Login if user is not signed in,
        /// or to the appropriate Home if they have the wrong role for this page.
        /// </summary>
        public static async Task<bool> RequireRoleAsync(Page page, string requiredRole)
        {
            if (!IsLoggedIn)
            {
                await Shell.Current.GoToAsync("//Login");
                return false;
            }
            if (Role != requiredRole)
            {
                await page.DisplayAlert("Access denied",
                    $"This page requires {requiredRole} role.", "OK");
                string home = Role == "Teacher" ? "//TeacherHome" : "//StudentHome";
                await Shell.Current.GoToAsync(home);
                return false;
            }
            return true;
        }

        public static void Clear()
        {
            Username = "";
            UserId = 0;
            Role = "";
            IsAdmin = false;
        }
    }
}
