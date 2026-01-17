using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using webTOsrv;
using System.Collections.Generic;
using System.Linq;

namespace Driver.Pages.Teacher
{
    public class AllStudentsModel : PageModel
    {
        private Service1Client srv = new Service1Client();

        public List<UserInfo> Students { get; set; }
        public bool IsAdmin { get; set; }

        public IActionResult OnGet()
        {
            // Check if user is logged in and is a teacher
            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(role) || role != "Teacher")
            {
                return RedirectToPage("/Login");
            }

            var username = HttpContext.Session.GetString("Username");
            var userId = HttpContext.Session.GetInt32("UserId");

            if (!userId.HasValue)
            {
                return RedirectToPage("/Login");
            }

            // Check if admin
            IsAdmin = CheckIfAdmin(username);

            if (IsAdmin)
            {
                // Admin sees ALL students
                var allUsers = srv.GetAllUsers();
                Students = allUsers != null ? allUsers.ToList() : new List<UserInfo>();
            }
            else
            {
                // Regular teacher sees only THEIR students
                var myStudents = srv.GetTeacherStudents(userId.Value);
                Students = myStudents != null ? myStudents.ToList() : new List<UserInfo>();
            }

            return Page();
        }

        private bool CheckIfAdmin(string username)
        {
            if (string.IsNullOrEmpty(username))
                return false;

            // Check by username
            string[] adminUsers = { "admin", "Admin", "ADMIN" };
            return adminUsers.Contains(username);
        }
    }
}