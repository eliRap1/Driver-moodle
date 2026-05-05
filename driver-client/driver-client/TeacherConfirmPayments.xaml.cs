using driver_client.driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace driver_client
{
    public partial class TeacherConfirmPayments : Page
    {
        public TeacherConfirmPayments()
        {
            InitializeComponent();
            LoadPendingPayments();
        }

        private void LoadPendingPayments()
        {
            try
            {
                var srv = new Service1Client();
                int teacherId = ClientSession.TeacherId;

                // Get all lessons for this teacher
                var lessons = srv.GetAllTeacherLessons(teacherId).ToList();

                // Filter to unpaid, non-cancelled lessons
                var unpaidLessons = lessons.Where(l => !l.paid && l.Canceled != 1).ToList();

                PendingCountText.Text = unpaidLessons.Count.ToString();

                LoadLessonCards(unpaidLessons, srv);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data:\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadLessonCards(List<Lessons> lessons, Service1Client srv)
        {
            PaymentsPanel.Children.Clear();

            if (lessons.Count == 0)
            {
                PaymentsPanel.Children.Add(new TextBlock
                {
                    Text = "No pending payments to confirm.",
                    Foreground = Brushes.LightGray,
                    FontStyle = FontStyles.Italic,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 30, 0, 0)
                });
                return;
            }

            // Sort by date
            var sortedLessons = lessons.OrderBy(l =>
            {
                DateTime dt;
                DateTime.TryParse($"{l.Date} {l.Time}", out dt);
                return dt;
            }).ToList();

            foreach (var lesson in sortedLessons)
            {
                PaymentsPanel.Children.Add(CreateLessonCard(lesson, srv));
            }
        }

        private Border CreateLessonCard(Lessons lesson, Service1Client srv)
        {
            var mainGrid = new Grid();
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Left side - Info
            var infoStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center };

            // Get student name
            string studentName = "Unknown Student";
            try
            {
                var student = srv.GetUserById(lesson.StudentId, "Student");
                if (student != null)
                {
                    studentName = student.Username;
                }
            }
            catch { }

            infoStack.Children.Add(new TextBlock
            {
                Text = studentName,
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.White
            });

            // Parse and format date
            string formattedDate = lesson.Date;
            try
            {
                DateTime dt;
                if (DateTime.TryParse($"{lesson.Date} {lesson.Time}", out dt))
                {
                    formattedDate = dt.ToString("dd/MM/yyyy HH:mm");
                }
            }
            catch { }

            infoStack.Children.Add(new TextBlock
            {
                Text = $"Lesson: {formattedDate}",
                FontSize = 12,
                Foreground = Brushes.Gray
            });

            // Get lesson price
            int price = 200;
            try
            {
                var teacher = srv.GetUserById(ClientSession.TeacherId, "Teacher");
                if (teacher != null && teacher.LessonPrice > 0)
                {
                    price = teacher.LessonPrice;
                }

                // Check for student-specific pricing
                var studentPrice = srv.GetStudentLessonPrice(lesson.StudentId);
                if (studentPrice > 0)
                {
                    price = studentPrice;
                }
            }
            catch { }

            infoStack.Children.Add(new TextBlock
            {
                Text = $"Amount: {price} NIS",
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(46, 204, 113))
            });

            Grid.SetColumn(infoStack, 0);
            mainGrid.Children.Add(infoStack);

            // Right side - Confirm button
            var confirmBtn = new Button
            {
                Content = "Confirm Paid",
                Background = new SolidColorBrush(Color.FromRgb(39, 174, 96)),
                Foreground = Brushes.White,
                FontWeight = FontWeights.Bold,
                Padding = new Thickness(15, 8, 15, 8),
                Cursor = System.Windows.Input.Cursors.Hand,
                BorderThickness = new Thickness(0),
                Tag = lesson.LessonId
            };
            confirmBtn.Click += ConfirmPayment_Click;

            Grid.SetColumn(confirmBtn, 1);
            mainGrid.Children.Add(confirmBtn);

            return new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(20, 36, 53)),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(15),
                Margin = new Thickness(0, 0, 0, 10),
                Child = mainGrid
            };
        }

        private void ConfirmPayment_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int lessonId)
            {
                var result = MessageBox.Show(
                    "Are you sure you want to confirm this payment?",
                    "Confirm Payment",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        ServiceGateway.Use(client => client.MarkLessonPaid(lessonId));

                        MessageBox.Show("Payment confirmed successfully!", "Success",
                            MessageBoxButton.OK, MessageBoxImage.Information);

                        // Refresh the list
                        LoadPendingPayments();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error confirming payment:\n{ex.Message}", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            page.Navigate(new TeacherUI());
        }
    }
}
