using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using webTOsrv;

namespace Driver.Pages.Student
{
    public class ViewLessonsModel : PageModel
    {
        private Service1Client srv = new Service1Client();

        public List<LessonViewModel> UpcomingLessons { get; set; } = new();
        public List<LessonViewModel> CompletedLessons { get; set; } = new();
        public int TotalLessons { get; set; }
        public int PaidLessons { get; set; }
        public int UnpaidLessons { get; set; }

        public class LessonViewModel
        {
            public int LessonId { get; set; }
            public string Date { get; set; } = "";
            public string Time { get; set; } = "";
            public bool IsPaid { get; set; }
            public DateTime DateTime { get; set; }
            public string FormattedDate => DateTime.ToString("dd/MM/yyyy");
        }

        public IActionResult OnGet()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Student")
            {
                return RedirectToPage("/Login");
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Login");
            }

            LoadLessons(userId.Value);
            return Page();
        }

        private void LoadLessons(int studentId)
        {
            try
            {
                var allLessons = srv.GetAllStudentLessons(studentId);
                var now = DateTime.Now;

                foreach (var lesson in allLessons)
                {
                    // Skip cancelled lessons
                    if (lesson.Canceled == 1)
                        continue;

                    DateTime lessonDateTime;
                    if (!DateTime.TryParse($"{lesson.Date} {lesson.Time}", out lessonDateTime))
                    {
                        if (!DateTime.TryParseExact($"{lesson.Date} {lesson.Time}",
                            new[] { "dd-MM-yyyy HH:mm", "dd/MM/yyyy HH:mm", "yyyy-MM-dd HH:mm" },
                            System.Globalization.CultureInfo.InvariantCulture,
                            System.Globalization.DateTimeStyles.None,
                            out lessonDateTime))
                        {
                            continue;
                        }
                    }

                    var viewModel = new LessonViewModel
                    {
                        LessonId = lesson.LessonId,
                        Date = lesson.Date,
                        Time = lesson.Time,
                        IsPaid = lesson.paid,
                        DateTime = lessonDateTime
                    };

                    if (lessonDateTime >= now)
                    {
                        UpcomingLessons.Add(viewModel);
                    }
                    else
                    {
                        CompletedLessons.Add(viewModel);
                    }

                    TotalLessons++;
                    if (lesson.paid)
                        PaidLessons++;
                    else
                        UnpaidLessons++;
                }

                UpcomingLessons = UpcomingLessons.OrderBy(l => l.DateTime).ToList();
                CompletedLessons = CompletedLessons.OrderByDescending(l => l.DateTime).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading lessons: {ex.Message}");
            }
        }
    }
}
