using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Linq;
using webTOsrv;

namespace Driver.Pages.Teacher
{
    public class TeacherHomeModel : PageModel
    {
        private Service1Client srv = new Service1Client();

        public string Username { get; set; }
        public int TotalStudents { get; set; }
        public int TotalTeachers { get; set; }
        public bool IsAdmin { get; set; }

        public IActionResult OnGet()
        {
            // Check if user is logged in and is a teacher
            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(role) || role != "Teacher")
            {
                return RedirectToPage("/Login");
            }

            Username = HttpContext.Session.GetString("Username");
            var userId = HttpContext.Session.GetInt32("UserId");

            if (!userId.HasValue)
            {
                return RedirectToPage("/Login");
            }

            // Check if this is an admin
            IsAdmin = CheckIfAdmin(Username);

            // Get statistics
            if (IsAdmin)
            {
                // Admin sees everything
                var allStudents = srv.GetAllUsers();
                var allTeachers = srv.GetAllTeacher();

                TotalStudents = allStudents != null ? allStudents.Count : 0;
                TotalTeachers = allTeachers != null ? allTeachers.Count : 0;
            }
            else
            {
                // Regular teacher sees only their students
                var myStudents = srv.GetTeacherStudents(userId.Value);
                TotalStudents = myStudents != null ? myStudents.Count() : 0;
                TotalTeachers = 0; // Regular teachers don't see other teachers
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