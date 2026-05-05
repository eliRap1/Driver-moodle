using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using webTOsrv;

namespace Driver.Pages.Student
{
    public class PaymentsModel : PageModel
    {
        private Service1Client srv = new Service1Client();

        public List<LessonViewModel> UnpaidLessons { get; set; } = new();
        public List<Payment> PaymentHistory { get; set; } = new();
        public int TotalDue { get; set; }
        public int TotalPaid { get; set; }
        public int UnpaidCount { get; set; }
        public int LessonPrice { get; set; } = 200;
        public string Message { get; set; } = "";
        public bool IsSuccess { get; set; }

        public class LessonViewModel
        {
            public int LessonId { get; set; }
            public string Date { get; set; } = "";
            public string Time { get; set; } = "";
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

            LoadData(userId.Value);
            return Page();
        }

        public IActionResult OnPostPay(int[] selectedLessons, string paymentMethod)
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

            if (selectedLessons == null || selectedLessons.Length == 0)
            {
                Message = "Please select at least one lesson to pay for.";
                IsSuccess = false;
                LoadData(userId.Value);
                return Page();
            }

            try
            {
                // Get teacher ID and lesson price
                int teacherId = srv.GetTeacherId(userId.Value);
                int price = LessonPrice;

                try
                {
                    var studentPrice = srv.GetStudentLessonPrice(userId.Value);
                    if (studentPrice > 0) price = studentPrice;
                }
                catch { }

                foreach (var lessonId in selectedLessons)
                {
                    var payment = new Payment
                    {
                        StudentID = userId.Value,
                        TeacherID = teacherId,
                        Amount = price,
                        PaymentDate = DateTime.Now,
                        PaymentMethod = paymentMethod,
                        NumberOfPayments = 1,
                        ParcialAmount = price,
                        paid = true,
                        LessonId = lessonId,
                        Status = "Paid"
                    };

                    srv.Pay(payment);
                }

                Message = $"Successfully paid for {selectedLessons.Length} lesson(s)!";
                IsSuccess = true;
            }
            catch (Exception ex)
            {
                Message = $"Error processing payment: {ex.Message}";
                IsSuccess = false;
            }

            LoadData(userId.Value);
            return Page();
        }

        private void LoadData(int studentId)
        {
            try
            {
                // Get lesson price
                int teacherId = srv.GetTeacherId(studentId);
                var teacher = srv.GetUserById(teacherId, "Teacher");
                if (teacher != null && teacher.LessonPrice > 0)
                {
                    LessonPrice = teacher.LessonPrice;
                }

                try
                {
                    var studentPrice = srv.GetStudentLessonPrice(studentId);
                    if (studentPrice > 0) LessonPrice = studentPrice;
                }
                catch { }

                // Load unpaid lessons
                var allLessons = srv.GetAllStudentLessons(studentId);
                foreach (var lesson in allLessons)
                {
                    if (lesson.Canceled == 1) continue;

                    if (!lesson.paid)
                    {
                        DateTime lessonDateTime = ParseLessonDateTime(lesson.Date, lesson.Time);

                        UnpaidLessons.Add(new LessonViewModel
                        {
                            LessonId = lesson.LessonId,
                            Date = lesson.Date,
                            Time = lesson.Time,
                            DateTime = lessonDateTime
                        });
                    }
                }

                UnpaidLessons = UnpaidLessons.OrderBy(l => l.DateTime).ToList();
                UnpaidCount = UnpaidLessons.Count;
                TotalDue = UnpaidCount * LessonPrice;

                // Load payment history
                var payments = srv.SelectPaymentByStudentID(studentId);
                if (payments != null)
                {
                    PaymentHistory = payments.OrderByDescending(p => p.PaymentDate).Take(10).ToList();
                    TotalPaid = payments.Where(p => p.paid).Sum(p => p.Amount);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading data: {ex.Message}");
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
