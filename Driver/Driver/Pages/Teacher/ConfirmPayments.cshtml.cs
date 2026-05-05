using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using webTOsrv;

namespace Driver.Pages.Teacher
{
    public class ConfirmPaymentsModel : PageModel
    {
        private Service1Client srv = new Service1Client();

        public List<LessonViewModel> PendingLessons { get; set; } = new();
        public int PendingCount => PendingLessons.Count;
        public string Message { get; set; } = "";
        public bool IsSuccess { get; set; }

        public class LessonViewModel
        {
            public int LessonId { get; set; }
            public int StudentId { get; set; }
            public string StudentName { get; set; } = "";
            public string Date { get; set; } = "";
            public string Time { get; set; } = "";
            public int Price { get; set; }
            public DateTime DateTime { get; set; }
            public string FormattedDate => DateTime.ToString("dd/MM/yyyy");
        }

        public IActionResult OnGet()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Teacher")
            {
                return RedirectToPage("/Login");
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Login");
            }

            LoadPendingLessons(userId.Value);
            return Page();
        }

        public IActionResult OnPostConfirm(int lessonId)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Teacher")
            {
                return RedirectToPage("/Login");
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Login");
            }

            try
            {
                srv.MarkLessonPaid(lessonId);
                Message = "Payment confirmed successfully!";
                IsSuccess = true;
            }
            catch (Exception ex)
            {
                Message = $"Error confirming payment: {ex.Message}";
                IsSuccess = false;
            }

            LoadPendingLessons(userId.Value);
            return Page();
        }

        public IActionResult OnPostConfirmAll()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Teacher")
            {
                return RedirectToPage("/Login");
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Login");
            }

            try
            {
                LoadPendingLessons(userId.Value);
                int count = 0;

                foreach (var lesson in PendingLessons)
                {
                    srv.MarkLessonPaid(lesson.LessonId);
                    count++;
                }

                Message = $"Successfully confirmed {count} payment(s)!";
                IsSuccess = true;
            }
            catch (Exception ex)
            {
                Message = $"Error confirming payments: {ex.Message}";
                IsSuccess = false;
            }

            LoadPendingLessons(userId.Value);
            return Page();
        }

        private void LoadPendingLessons(int teacherId)
        {
            PendingLessons.Clear();

            try
            {
                // Get teacher's default lesson price
                int defaultPrice = 200;
                var teacher = srv.GetUserById(teacherId, "Teacher");
                if (teacher != null && teacher.LessonPrice > 0)
                {
                    defaultPrice = teacher.LessonPrice;
                }

                // Get all lessons for this teacher
                var lessons = srv.GetAllTeacherLessons(teacherId);

                foreach (var lesson in lessons)
                {
                    // Skip paid and cancelled lessons
                    if (lesson.paid || lesson.Canceled == 1)
                        continue;

                    DateTime lessonDateTime = ParseLessonDateTime(lesson.Date, lesson.Time);

                    // Get student name
                    string studentName = "Unknown";
                    int price = defaultPrice;

                    try
                    {
                        var student = srv.GetUserById(lesson.StudentId, "Student");
                        if (student != null)
                        {
                            studentName = student.Username;
                        }

                        // Check for student-specific pricing
                        var studentPrice = srv.GetStudentLessonPrice(lesson.StudentId);
                        if (studentPrice > 0)
                        {
                            price = studentPrice;
                        }
                    }
                    catch { }

                    PendingLessons.Add(new LessonViewModel
                    {
                        LessonId = lesson.LessonId,
                        StudentId = lesson.StudentId,
                        StudentName = studentName,
                        Date = lesson.Date,
                        Time = lesson.Time,
                        Price = price,
                        DateTime = lessonDateTime
                    });
                }

                PendingLessons = PendingLessons.OrderBy(l => l.DateTime).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading pending lessons: {ex.Message}");
            }
        }

        private static DateTime ParseLessonDateTime(string date, string time)
        {
            string[] formats = { "yyyy-MM-dd HH:mm", "dd-MM-yyyy HH:mm", "dd/MM/yyyy HH:mm", "yyyy-MM-dd H:mm", "MM/dd/yyyy HH:mm:ss" };
            string combined = $"{date} {time}";
            if (DateTime.TryParseExact(combined, formats,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out var dt))
                return dt;
            if (DateTime.TryParse(combined, System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out dt))
                return dt;
            DateTime.TryParse(combined, out dt);
            return dt;
        }
    }
}
