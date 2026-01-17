using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using webTOsrv;
using System.Collections.Generic;
using System.Linq;

namespace Driver.Pages.Teacher
{
    public class AllTeachersModel : PageModel
    {
        private Service1Client srv = new Service1Client();

        public List<UserInfo> Teachers { get; set; }
        public List<string> Reviews { get; set; }
        public string SelectedTeacherName { get; set; }

        public IActionResult OnGet()
        {
            // Check if user is logged in and is a teacher
            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(role) || role != "Teacher")
            {
                return RedirectToPage("/Login");
            }

            LoadTeachers();
            return Page();
        }

        public IActionResult OnPostViewReviews(int teacherId)
        {
            // Check if user is logged in and is a teacher
            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(role) || role != "Teacher")
            {
                return RedirectToPage("/Login");
            }

            LoadTeachers();

            // Get reviews for the selected teacher
            Reviews = srv.GetTeacherReviews(teacherId).ToList();

            // Get teacher name
            var teacher = Teachers.FirstOrDefault(t => t.Id == teacherId);
            SelectedTeacherName = teacher?.Username ?? "Unknown Teacher";

            return Page();
        }

        private void LoadTeachers()
        {
            var allTeachers = srv.GetAllTeacher();
            Teachers = allTeachers?.ToList() ?? new List<UserInfo>();
        }
    }
}