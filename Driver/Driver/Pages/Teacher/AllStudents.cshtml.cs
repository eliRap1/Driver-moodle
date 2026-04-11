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
        public Dictionary<int, int> EffectivePrices { get; set; } = new();
        public string Message { get; set; }
        public bool IsSuccess { get; set; }

        public IActionResult OnGet()
        {
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

            LoadData(userId.Value, username);
            return Page();
        }

        public IActionResult OnPostSetPrice(int studentId, int price)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToPage("/Login");
            try
            {
                srv.SetStudentLessonPrice(studentId, price);
                Message = $"Price updated to ₪{price}.";
                IsSuccess = true;
            }
            catch (Exception ex) { Message = ex.Message; IsSuccess = false; }
            LoadData(userId.Value, HttpContext.Session.GetString("Username"));
            return Page();
        }

        public IActionResult OnPostSetDiscount(int studentId, int discountPercent)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToPage("/Login");
            try
            {
                srv.SetStudentDiscount(studentId, discountPercent);
                Message = $"Discount set to {discountPercent}%.";
                IsSuccess = true;
            }
            catch (Exception ex) { Message = ex.Message; IsSuccess = false; }
            LoadData(userId.Value, HttpContext.Session.GetString("Username"));
            return Page();
        }

        private void LoadData(int userId, string username)
        {
            IsAdmin = CheckIfAdmin(username);

            if (IsAdmin)
            {
                var allUsers = srv.GetAllUsers();
                Students = allUsers != null ? allUsers.ToList() : new List<UserInfo>();
            }
            else
            {
                var myStudents = srv.GetTeacherStudents(userId);
                Students = myStudents != null ? myStudents.ToList() : new List<UserInfo>();
            }

            foreach (var s in Students)
            {
                try { EffectivePrices[s.Id] = srv.GetEffectiveLessonPrice(s.Id); }
                catch { EffectivePrices[s.Id] = 200; }
            }
        }

        private bool CheckIfAdmin(string username)
        {
            if (string.IsNullOrEmpty(username))
                return false;

            string[] adminUsers = { "admin", "Admin", "ADMIN" };
            return adminUsers.Contains(username);
        }
    }
}