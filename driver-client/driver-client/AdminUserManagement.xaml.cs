using driver_client.driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace driver_client
{
    public partial class AdminUserManagement : Page
    {
        private bool showingStudents = true;
        private UserInfo selectedUser = null;

        public AdminUserManagement()
        {
            InitializeComponent();
            LoadUsers();
        }

        private void LoadUsers()
        {
            try
            {
                var srv = new Service1Client();
                UsersPanel.Children.Clear();

                if (showingStudents)
                {
                    var students = srv.GetAllUsers();
                    foreach (var student in students)
                    {
                        UsersPanel.Children.Add(CreateUserCard(student, false));
                    }

                    if (students.Count == 0)
                    {
                        UsersPanel.Children.Add(new TextBlock
                        {
                            Text = "No students found",
                            Foreground = Brushes.LightGray,
                            FontStyle = FontStyles.Italic,
                            Margin = new Thickness(10)
                        });
                    }
                }
                else
                {
                    var teachers = srv.GetAllTeacher();
                    foreach (var teacher in teachers)
                    {
                        UsersPanel.Children.Add(CreateUserCard(teacher, true));
                    }

                    if (teachers.Count == 0)
                    {
                        UsersPanel.Children.Add(new TextBlock
                        {
                            Text = "No teachers found",
                            Foreground = Brushes.LightGray,
                            FontStyle = FontStyles.Italic,
                            Margin = new Thickness(10)
                        });
                    }
                }

                // Update tab button styles
                StudentsTabBtn.Background = showingStudents
                    ? new SolidColorBrush(Color.FromRgb(0, 224, 255))
                    : new SolidColorBrush(Color.FromRgb(27, 45, 71));
                StudentsTabBtn.Foreground = showingStudents ? Brushes.Black : Brushes.White;

                TeachersTabBtn.Background = !showingStudents
                    ? new SolidColorBrush(Color.FromRgb(0, 224, 255))
                    : new SolidColorBrush(Color.FromRgb(27, 45, 71));
                TeachersTabBtn.Foreground = !showingStudents ? Brushes.Black : Brushes.White;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading users:\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private Border CreateUserCard(UserInfo user, bool isTeacher)
        {
            var stack = new StackPanel { Orientation = Orientation.Horizontal };

            // User icon
            stack.Children.Add(new TextBlock
            {
                Text = isTeacher ? "👨‍🏫" : "👤",
                FontSize = 24,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0)
            });

            // User info
            var infoStack = new StackPanel();
            infoStack.Children.Add(new TextBlock
            {
                Text = user.Username,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White
            });

            infoStack.Children.Add(new TextBlock
            {
                Text = user.Email,
                FontSize = 12,
                Foreground = Brushes.LightGray
            });

            // Status
            if (!isTeacher)
            {
                infoStack.Children.Add(new TextBlock
                {
                    Text = user.Confirmed ? "✓ Confirmed" : "⏳ Pending",
                    FontSize = 11,
                    Foreground = user.Confirmed ? Brushes.LightGreen : Brushes.Orange
                });
            }
            else
            {
                infoStack.Children.Add(new TextBlock
                {
                    Text = $"Rating: {user.Rating:F1}/5",
                    FontSize = 11,
                    Foreground = new SolidColorBrush(Color.FromRgb(0, 224, 255))
                });
            }

            stack.Children.Add(infoStack);

            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(26, 46, 80)),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12),
                Margin = new Thickness(0, 0, 0, 8),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0, 192, 204)),
                BorderThickness = new Thickness(1),
                Child = stack,
                Tag = user,
                Cursor = System.Windows.Input.Cursors.Hand
            };

            border.MouseLeftButtonDown += (s, e) => SelectUser(user, isTeacher);

            return border;
        }

        private void SelectUser(UserInfo user, bool isTeacher)
        {
            selectedUser = user;
            UserDetailsPanel.Children.Clear();

            // Header
            UserDetailsPanel.Children.Add(new TextBlock
            {
                Text = isTeacher ? "👨‍🏫 Teacher Details" : "👤 Student Details",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(0, 224, 255)),
                Margin = new Thickness(0, 0, 0, 15)
            });

            // ID
            AddDetailRow("ID:", user.Id.ToString());

            // Username
            AddDetailRow("Username:", user.Username);

            // Email
            AddDetailRow("Email:", user.Email);

            // Phone
            AddDetailRow("Phone:", user.Phone);

            if (!isTeacher)
            {
                // Teacher ID
                AddDetailRow("Teacher ID:", user.TeacherId.ToString());

                // Confirmed status
                AddDetailRow("Status:", user.Confirmed ? "Confirmed" : "Pending");

                // Confirm button if not confirmed
                if (!user.Confirmed)
                {
                    var confirmBtn = new Button
                    {
                        Content = "✓ Confirm Student",
                        Background = new SolidColorBrush(Color.FromRgb(46, 204, 113)),
                        Foreground = Brushes.White,
                        FontWeight = FontWeights.Bold,
                        Height = 35,
                        Margin = new Thickness(0, 15, 0, 5),
                        Cursor = System.Windows.Input.Cursors.Hand
                    };
                    confirmBtn.Click += (s, e) => ConfirmStudent(user);
                    UserDetailsPanel.Children.Add(confirmBtn);
                }
            }
            else
            {
                // Rating
                AddDetailRow("Rating:", $"{user.Rating:F1}/5");

                // Lesson Price
                AddDetailRow("Lesson Price:", $"{user.LessonPrice} ₪");
            }

            // View Payments button
            var paymentsBtn = new Button
            {
                Content = "💰 View Payments",
                Background = new SolidColorBrush(Color.FromRgb(52, 152, 219)),
                Foreground = Brushes.White,
                FontWeight = FontWeights.Bold,
                Height = 35,
                Margin = new Thickness(0, 10, 0, 5),
                Cursor = System.Windows.Input.Cursors.Hand,
                Tag = user
            };
            paymentsBtn.Click += ViewPayments_Click;
            UserDetailsPanel.Children.Add(paymentsBtn);

            // View Lessons button
            var lessonsBtn = new Button
            {
                Content = "📅 View Lessons",
                Background = new SolidColorBrush(Color.FromRgb(155, 89, 182)),
                Foreground = Brushes.White,
                FontWeight = FontWeights.Bold,
                Height = 35,
                Margin = new Thickness(0, 5, 0, 5),
                Cursor = System.Windows.Input.Cursors.Hand,
                Tag = user
            };
            lessonsBtn.Click += ViewLessons_Click;
            UserDetailsPanel.Children.Add(lessonsBtn);
        }

        private void AddDetailRow(string label, string value)
        {
            var stack = new StackPanel { Margin = new Thickness(0, 0, 0, 8) };
            stack.Children.Add(new TextBlock
            {
                Text = label,
                FontSize = 12,
                Foreground = Brushes.Gray
            });
            stack.Children.Add(new TextBlock
            {
                Text = value,
                FontSize = 14,
                Foreground = Brushes.White,
                FontWeight = FontWeights.SemiBold
            });
            UserDetailsPanel.Children.Add(stack);
        }

        private void ConfirmStudent(UserInfo student)
        {
            try
            {
                var srv = new Service1Client();
                srv.TeacherConfirm(student.Id, student.TeacherId);
                MessageBox.Show($"Student {student.Username} has been confirmed!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                LoadUsers();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error confirming student:\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ViewPayments_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is UserInfo user)
            {
                try
                {
                    var srv = new Service1Client();
                    List<Payment> payments;

                    if (showingStudents)
                    {
                        payments = srv.SelectPaymentByStudentID(user.Id).ToList();
                    }
                    else
                    {
                        payments = srv.SelectPaymentByTeacherID(user.Id).ToList();
                    }

                    string message = $"Payments for {user.Username}:\n\n";

                    if (payments.Count == 0)
                    {
                        message += "No payments found.";
                    }
                    else
                    {
                        decimal total = 0;
                        foreach (var p in payments.Take(10))
                        {
                            string status = p.paid ? "✓ Paid" : "⏳ Pending";
                            message += $"• {p.PaymentDate:dd/MM/yyyy} - {p.Amount} ₪ - {status}\n";
                            if (p.paid) total += p.Amount;
                        }
                        message += $"\nTotal Paid: {total} ₪";

                        if (payments.Count > 10)
                        {
                            message += $"\n\n(Showing 10 of {payments.Count} payments)";
                        }
                    }

                    MessageBox.Show(message, "Payment History", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading payments:\n{ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ViewLessons_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is UserInfo user)
            {
                try
                {
                    var srv = new Service1Client();
                    List<Lessons> lessons;

                    if (showingStudents)
                    {
                        lessons = srv.GetAllStudentLessons(user.Id).ToList();
                    }
                    else
                    {
                        lessons = srv.GetAllTeacherLessons(user.Id).ToList();
                    }

                    string message = $"Lessons for {user.Username}:\n\n";

                    if (lessons.Count == 0)
                    {
                        message += "No lessons found.";
                    }
                    else
                    {
                        int completed = lessons.Count(l => l.Canceled != 1 && DateTime.TryParse(l.Date, out var d) && d < DateTime.Today);
                        int upcoming = lessons.Count(l => l.Canceled != 1 && DateTime.TryParse(l.Date, out var d) && d >= DateTime.Today);
                        int cancelled = lessons.Count(l => l.Canceled == 1);

                        message += $"Total: {lessons.Count}\n";
                        message += $"✓ Completed: {completed}\n";
                        message += $"📅 Upcoming: {upcoming}\n";
                        message += $"✗ Cancelled: {cancelled}";
                    }

                    MessageBox.Show(message, "Lessons Summary", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading lessons:\n{ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void StudentsTab_Click(object sender, RoutedEventArgs e)
        {
            showingStudents = true;
            selectedUser = null;
            UserDetailsPanel.Children.Clear();
            UserDetailsPanel.Children.Add(new TextBlock
            {
                Text = "Select a user to view details",
                Foreground = Brushes.LightGray,
                FontStyle = FontStyles.Italic,
                HorizontalAlignment = HorizontalAlignment.Center
            });
            LoadUsers();
        }

        private void TeachersTab_Click(object sender, RoutedEventArgs e)
        {
            showingStudents = false;
            selectedUser = null;
            UserDetailsPanel.Children.Clear();
            UserDetailsPanel.Children.Add(new TextBlock
            {
                Text = "Select a user to view details",
                Foreground = Brushes.LightGray,
                FontStyle = FontStyles.Italic,
                HorizontalAlignment = HorizontalAlignment.Center
            });
            LoadUsers();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            page.Navigate(new AdminDashboard());
        }
    }
}
