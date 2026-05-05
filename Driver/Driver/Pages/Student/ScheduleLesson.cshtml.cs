using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using webTOsrv;

namespace Driver.Pages.Student
{
    public class ScheduleLessonModel : PageModel
    {
        private Service1Client srv = new Service1Client();

        public List<string> AvailableTimes { get; set; } = new();
        public DateTime SelectedDate { get; set; } = DateTime.Today.AddDays(1);
        public string Message { get; set; } = "";
        public bool IsSuccess { get; set; }
        public int TeacherId { get; set; }
        public string TeacherName { get; set; } = "";

        public IActionResult OnGet(string date)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Student") return RedirectToPage("/Login");
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToPage("/Login");

            if (!string.IsNullOrEmpty(date) &&
                DateTime.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
            {
                SelectedDate = parsed;
            }

            LoadTimes(userId.Value);
            return Page();
        }

        public IActionResult OnPostBook(string date, string time)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Student") return RedirectToPage("/Login");
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToPage("/Login");

            if (string.IsNullOrEmpty(date) || string.IsNullOrEmpty(time))
            {
                Message = "Pick a date and time first.";
                IsSuccess = false;
                LoadTimes(userId.Value);
                return Page();
            }

            try
            {
                srv.AddLessonForStudent(userId.Value, date, time);
                Message = $"Lesson booked: {date} at {time}.";
                IsSuccess = true;

                if (DateTime.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
                    SelectedDate = d;
            }
            catch (Exception ex)
            {
                Message = $"Error booking lesson: {ex.Message}";
                IsSuccess = false;
            }

            LoadTimes(userId.Value);
            return Page();
        }

        private void LoadTimes(int studentId)
        {
            try
            {
                TeacherId = srv.GetTeacherId(studentId);
                if (TeacherId > 0)
                {
                    var teacher = srv.GetUserById(TeacherId, "Teacher");
                    if (teacher != null) TeacherName = teacher.Username ?? "";
                }

                Calendars cal = null;
                try { cal = srv.GetTeacherCalendar(TeacherId); } catch { }

                TimeSpan start = TimeSpan.FromHours(8);
                TimeSpan end = TimeSpan.FromHours(20);
                List<string> availableDays = null;

                if (cal != null)
                {
                    if (!TimeSpan.TryParse(cal.StartTime, out start)) start = TimeSpan.FromHours(8);
                    if (!TimeSpan.TryParse(cal.EndTime, out end)) end = TimeSpan.FromHours(20);
                    if (cal.AvailableDays != null) availableDays = cal.AvailableDays.ToList();
                }

                if (availableDays != null && availableDays.Count > 0 &&
                    !availableDays.Contains(SelectedDate.DayOfWeek.ToString()))
                {
                    AvailableTimes = new List<string>();
                    return;
                }

                var taken = new HashSet<string>();
                try
                {
                    var existing = srv.GetAllStudentLessons(studentId);
                    string isoDate = SelectedDate.ToString("yyyy-MM-dd");
                    foreach (var l in existing)
                    {
                        if (l.Canceled == 1) continue;
                        if (l.Date == isoDate || l.Date == SelectedDate.ToString("dd-MM-yyyy") ||
                            l.Date == SelectedDate.ToString("dd/MM/yyyy"))
                        {
                            taken.Add(l.Time);
                        }
                    }
                }
                catch { }

                AvailableTimes = new List<string>();
                for (var t = start; t < end; t = t.Add(TimeSpan.FromHours(1)))
                {
                    string s = t.ToString(@"hh\:mm");
                    if (!taken.Contains(s)) AvailableTimes.Add(s);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ScheduleLesson LoadTimes error: {ex.Message}");
            }
        }
    }
}
