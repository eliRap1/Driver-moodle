using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using webTOsrv;

namespace Driver.Pages.Teacher
{
    public class PaymentReportsModel : PageModel
    {
        private Service1Client srv = new Service1Client();

        public int TotalEarnings { get; set; }
        public int ThisMonthEarnings { get; set; }
        public int PendingAmount { get; set; }
        public int StudentsWithDebt { get; set; }
        public List<PaymentViewModel> RecentPayments { get; set; } = new();
        public List<DebtStudentViewModel> DebtStudents { get; set; } = new();

        public class PaymentViewModel
        {
            public int PaymentId { get; set; }
            public string StudentName { get; set; } = "";
            public DateTime PaymentDate { get; set; }
            public int Amount { get; set; }
            public string PaymentMethod { get; set; } = "";
            public bool IsPaid { get; set; }
        }

        public class DebtStudentViewModel
        {
            public int StudentId { get; set; }
            public string StudentName { get; set; } = "";
            public int UnpaidLessons { get; set; }
            public int TotalOwed { get; set; }
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

            LoadData(userId.Value);
            return Page();
        }

        private void LoadData(int teacherId)
        {
            try
            {
                // Get teacher's default lesson price
                int defaultPrice = 200;
                var teacher = srv.GetUserById(teacherId, "Teacher");
                if (teacher != null && teacher.LessonPrice > 0)
                {
                    defaultPrice = teacher.LessonPrice;
                }

                // Load payments
                var payments = srv.SelectPaymentByTeacherID(teacherId);
                if (payments != null)
                {
                    TotalEarnings = payments.Where(p => p.paid).Sum(p => p.Amount);
                    ThisMonthEarnings = payments
                        .Where(p => p.paid && p.PaymentDate.Month == DateTime.Now.Month && p.PaymentDate.Year == DateTime.Now.Year)
                        .Sum(p => p.Amount);

                    // Recent payments
                    foreach (var payment in payments.OrderByDescending(p => p.PaymentDate).Take(15))
                    {
                        string studentName = "Unknown";
                        try
                        {
                            var student = srv.GetUserById(payment.StudentID, "Student");
                            if (student != null) studentName = student.Username;
                        }
                        catch { }

                        RecentPayments.Add(new PaymentViewModel
                        {
                            PaymentId = payment.PaymentID,
                            StudentName = studentName,
                            PaymentDate = payment.PaymentDate,
                            Amount = payment.Amount,
                            PaymentMethod = payment.PaymentMethod ?? "N/A",
                            IsPaid = payment.paid
                        });
                    }
                }

                // Load lessons to calculate pending and debt
                var lessons = srv.GetAllTeacherLessons(teacherId);
                var unpaidByStudent = new Dictionary<int, int>();

                foreach (var lesson in lessons)
                {
                    if (lesson.Canceled == 1) continue;

                    if (!lesson.paid)
                    {
                        if (!unpaidByStudent.ContainsKey(lesson.StudentId))
                            unpaidByStudent[lesson.StudentId] = 0;
                        unpaidByStudent[lesson.StudentId]++;
                    }
                }

                int totalUnpaidLessons = unpaidByStudent.Values.Sum();
                PendingAmount = totalUnpaidLessons * defaultPrice;
                StudentsWithDebt = unpaidByStudent.Count;

                // Build debt students list
                foreach (var kvp in unpaidByStudent)
                {
                    string studentName = "Unknown";
                    int price = defaultPrice;

                    try
                    {
                        var student = srv.GetUserById(kvp.Key, "Student");
                        if (student != null) studentName = student.Username;

                        var studentPrice = srv.GetStudentLessonPrice(kvp.Key);
                        if (studentPrice > 0) price = studentPrice;
                    }
                    catch { }

                    DebtStudents.Add(new DebtStudentViewModel
                    {
                        StudentId = kvp.Key,
                        StudentName = studentName,
                        UnpaidLessons = kvp.Value,
                        TotalOwed = kvp.Value * price
                    });
                }

                DebtStudents = DebtStudents.OrderByDescending(s => s.TotalOwed).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading data: {ex.Message}");
            }
        }
    }
}
