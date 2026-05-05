using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using webTOsrv;

namespace Driver.Pages.Student
{
    public class WriteReviewModel : PageModel
    {
        private Service1Client srv = new Service1Client();

        public int TeacherId { get; set; }
        public string TeacherName { get; set; } = "";
        public string Message { get; set; } = "";
        public bool IsSuccess { get; set; }

        public IActionResult OnGet()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Student") return RedirectToPage("/Login");
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToPage("/Login");

            LoadTeacher(userId.Value);
            return Page();
        }

        public IActionResult OnPostSubmit(int rating, string review)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Student") return RedirectToPage("/Login");
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToPage("/Login");

            if (rating < 1 || rating > 5)
            {
                Message = "Pick a rating between 1 and 5.";
                IsSuccess = false;
                LoadTeacher(userId.Value);
                return Page();
            }

            if (string.IsNullOrWhiteSpace(review))
            {
                Message = "Write a review before submitting.";
                IsSuccess = false;
                LoadTeacher(userId.Value);
                return Page();
            }

            try
            {
                LoadTeacher(userId.Value);
                if (TeacherId <= 0)
                {
                    Message = "No teacher assigned.";
                    IsSuccess = false;
                    return Page();
                }

                srv.UpdateRating(TeacherId, rating, review);
                Message = "Review submitted. Thanks!";
                IsSuccess = true;
            }
            catch (Exception ex)
            {
                Message = $"Error submitting review: {ex.Message}";
                IsSuccess = false;
            }

            return Page();
        }

        private void LoadTeacher(int studentId)
        {
            try
            {
                TeacherId = srv.GetTeacherId(studentId);
                if (TeacherId > 0)
                {
                    var t = srv.GetUserById(TeacherId, "Teacher");
                    if (t != null) TeacherName = t.Username ?? "";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"WriteReview LoadTeacher error: {ex.Message}");
            }
        }
    }
}
