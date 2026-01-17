using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using webTOsrv;

namespace Driver.Pages.Student
{
    public class StudentHomeModel : PageModel
    {
        private Service1Client srv = new Service1Client();

        public string Username { get; set; }
        public int TeacherId { get; set; }
        public bool IsConfirmed { get; set; }

        public IActionResult OnGet()
        {
            // Check if user is logged in and is a student
            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(role) || role != "Student")
            {
                return RedirectToPage("/Login");
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToPage("/Login");
            }

            Username = HttpContext.Session.GetString("Username");

            // Get student information
            var student = srv.GetUserById(userId.Value, "Student");
            if (student != null)
            {
                TeacherId = student.TeacherId;
                IsConfirmed = student.Confirmed;
            }

            return Page();
        }
    }
}