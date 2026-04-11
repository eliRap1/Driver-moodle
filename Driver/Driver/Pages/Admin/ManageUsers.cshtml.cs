using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using webTOsrv;
using System.Collections.Generic;
using System.Linq;

namespace Driver.Pages.Admin
{
    public class ManageUsersModel : PageModel
    {
        private Service1Client srv = new Service1Client();

        public List<UserInfo> Students { get; set; } = new();
        public List<UserInfo> Teachers { get; set; } = new();
        public string Message { get; set; } = "";
        public bool IsSuccess { get; set; }

        public IActionResult OnGet()
        {
            if (HttpContext.Session.GetString("Role") != "Teacher" || !IsAdmin())
                return RedirectToPage("/Login");
            LoadData();
            return Page();
        }

        public IActionResult OnPostUpdateStudent(int studentId, string email, string phone, int teacherId)
        {
            if (!IsAdmin()) return RedirectToPage("/Login");
            try
            {
                srv.UpdateStudentCredentials(studentId, email, phone, teacherId);
                Message = "Student updated successfully.";
                IsSuccess = true;
            }
            catch (Exception ex)
            {
                Message = $"Error: {ex.Message}";
                IsSuccess = false;
            }
            LoadData();
            return Page();
        }

        public IActionResult OnPostResetPassword(int userId, string table, string newPassword)
        {
            if (!IsAdmin()) return RedirectToPage("/Login");
            try
            {
                srv.ResetPassword(userId, table, newPassword);
                Message = "Password reset successfully.";
                IsSuccess = true;
            }
            catch (Exception ex)
            {
                Message = $"Error: {ex.Message}";
                IsSuccess = false;
            }
            LoadData();
            return Page();
        }

        private bool IsAdmin()
        {
            var username = HttpContext.Session.GetString("Username");
            return srv.IsUserAdmin(username ?? "");
        }

        private void LoadData()
        {
            try
            {
                var allStudents = srv.GetAllUsers();
                if (allStudents != null)
                    Students = allStudents.ToList();
            }
            catch { }
            try
            {
                var allTeachers = srv.GetAllTeacher();
                if (allTeachers != null)
                    Teachers = allTeachers.ToList();
            }
            catch { }
        }
    }
}
