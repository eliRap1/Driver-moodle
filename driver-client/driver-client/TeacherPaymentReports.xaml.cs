using driver_client.driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace driver_client
{
    public partial class TeacherPaymentReports : Page
    {
        public TeacherPaymentReports()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                var srv = new Service1Client();

                // Get all payments for this teacher
                var payments = srv.SelectPaymentByTeacherID(LogIn.sign.Id).ToList();

                // Get all students and their lessons for debt calculation
                var students = srv.GetTeacherStudents(LogIn.sign.Id).ToList();
                var lessons = srv.GetAllTeacherLessons(LogIn.sign.Id).ToList();

                // Calculate totals
                int totalEarnings = payments.Where(p => p.paid).Sum(p => p.Amount);
                TotalEarningsText.Text = $"{totalEarnings} ₪";

                // This month's earnings
                var thisMonth = payments
                    .Where(p => p.paid && p.PaymentDate.Month == DateTime.Now.Month && p.PaymentDate.Year == DateTime.Now.Year)
                    .Sum(p => p.Amount);
                ThisMonthText.Text = $"{thisMonth} ₪";

                // Pending payments (unpaid lessons)
                var teacher = srv.GetUserById(LogIn.sign.Id, "Teacher");
                int lessonPrice = teacher?.LessonPrice > 0 ? teacher.LessonPrice : 200;

                int unpaidLessonsCount = lessons.Count(l => !l.paid && l.Canceled != 1);
                int pendingAmount = unpaidLessonsCount * lessonPrice;
                PendingText.Text = $"{pendingAmount} ₪";

                // Students with debt (students who have unpaid lessons)
                var studentsWithDebt = students.Count(s =>
                {
                    var studentLessons = srv.GetAllStudentLessons(s.StudentId);
                    return studentLessons.Any(l => !l.paid && l.Canceled != 1);
                });
                StudentsWithDebtText.Text = studentsWithDebt.ToString();

                // Load recent payments
                LoadPayments(payments);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data:\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadPayments(List<Payment> payments)
        {
            PaymentsPanel.Children.Clear();

            if (payments.Count == 0)
            {
                PaymentsPanel.Children.Add(new TextBlock
                {
                    Text = "No payments recorded yet.",
                    Foreground = Brushes.LightGray,
                    FontStyle = FontStyles.Italic,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 30, 0, 0)
                });
                return;
            }

            // Sort by date descending
            var sortedPayments = payments.OrderByDescending(p => p.PaymentDate).ToList();

            foreach (var payment in sortedPayments)
            {
                PaymentsPanel.Children.Add(CreatePaymentCard(payment));
            }
        }

        private Border CreatePaymentCard(Payment payment)
        {
            var mainStack = new StackPanel { Orientation = Orientation.Horizontal };

            // Status icon
            mainStack.Children.Add(new TextBlock
            {
                Text = payment.paid ? "✓" : "⏳",
                FontSize = 20,
                Foreground = payment.paid ? Brushes.LightGreen : Brushes.Orange,
                VerticalAlignment = VerticalAlignment.Center,
                Width = 35
            });

            // Payment info
            var infoStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center };

            // Get student name
            string studentName = "Unknown";
            try
            {
                var srv = new Service1Client();
                var student = srv.GetUserById(payment.StudentID, "Student");
                if (student != null)
                {
                    studentName = student.Username;
                }
            }
            catch { }

            infoStack.Children.Add(new TextBlock
            {
                Text = studentName,
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.White
            });

            infoStack.Children.Add(new TextBlock
            {
                Text = $"{payment.PaymentDate:dd/MM/yyyy HH:mm} • {payment.PaymentMethod ?? "N/A"}",
                FontSize = 11,
                Foreground = Brushes.Gray
            });

            if (!string.IsNullOrEmpty(payment.Notes))
            {
                infoStack.Children.Add(new TextBlock
                {
                    Text = payment.Notes,
                    FontSize = 11,
                    Foreground = Brushes.LightGray,
                    FontStyle = FontStyles.Italic
                });
            }

            mainStack.Children.Add(infoStack);

            // Amount (right aligned)
            var amountText = new TextBlock
            {
                Text = $"{payment.Amount} ₪",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = payment.paid
                    ? new SolidColorBrush(Color.FromRgb(46, 204, 113))
                    : new SolidColorBrush(Color.FromRgb(243, 156, 18)),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(20, 0, 0, 0)
            };

            var grid = new Grid();
            grid.Children.Add(mainStack);
            grid.Children.Add(amountText);

            return new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(20, 36, 53)),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12),
                Margin = new Thickness(0, 0, 0, 8),
                Child = grid
            };
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            page.Navigate(new TeacherUI());
        }
    }
}
