using driver_client.driver;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace driver_client
{
    public partial class AdminDashboard : Page
    {
        private DispatcherTimer refreshTimer;

        public AdminDashboard()
        {
            InitializeComponent();
            LoadDashboard();

            // Auto-refresh every 30 seconds
            refreshTimer = new DispatcherTimer();
            refreshTimer.Interval = TimeSpan.FromSeconds(30);
            refreshTimer.Tick += (s, e) => LoadDashboard();
            refreshTimer.Start();
        }

        private void LoadDashboard()
        {
            try
            {
                var srv = new Service1Client();

                // Welcome message
                WelcomeText.Text = $"Welcome, {LogIn.sign.Username}";

                // Statistics
                var allStudents = srv.GetAllUsers();
                var allTeachers = srv.GetAllTeacher();
                var allTickets = srv.GetAllTickets();

                TotalUsersText.Text = (allStudents.Count + allTeachers.Count).ToString();

                int openTickets = allTickets.Count(t => t.Status == "Open" || t.Status == "In Progress");
                OpenTicketsText.Text = openTickets.ToString();

                // Lessons today
                var today = DateTime.Today;
                int lessonsToday = 0;
                foreach (var teacher in allTeachers)
                {
                    var lessons = srv.GetAllTeacherLessons(teacher.Id);
                    lessonsToday += lessons.Count(l =>
                        DateTime.TryParse(l.Date, out DateTime lessonDate) &&
                        lessonDate.Date == today);
                }
                LessonsTodayText.Text = lessonsToday.ToString();

                // Pending payments (placeholder - implement when payment system is ready)
                PendingPaymentsText.Text = "N/A";

                // Recent Activity
                LoadRecentActivity(allTickets.ToList());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading dashboard:\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadRecentActivity(System.Collections.Generic.List<SupportTicket> tickets)
        {
            ActivityPanel.Children.Clear();

            // Get recent tickets (last 5)
            var recentTickets = tickets.OrderByDescending(t => t.CreatedAt).Take(5);

            if (!recentTickets.Any())
            {
                ActivityPanel.Children.Add(new TextBlock
                {
                    Text = "No recent activity",
                    Foreground = Brushes.LightGray,
                    FontStyle = FontStyles.Italic
                });
                return;
            }

            foreach (var ticket in recentTickets)
            {
                var activityItem = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(26, 46, 80)),
                    CornerRadius = new CornerRadius(5),
                    Padding = new Thickness(10),
                    Margin = new Thickness(0, 0, 0, 8)
                };

                var stack = new StackPanel();

                var header = new StackPanel { Orientation = Orientation.Horizontal };
                header.Children.Add(new TextBlock
                {
                    Text = $"Ticket #{ticket.TicketId}",
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Color.FromRgb(0, 224, 255)),
                    Margin = new Thickness(0, 0, 10, 0)
                });

                header.Children.Add(new TextBlock
                {
                    Text = ticket.Status,
                    Foreground = GetStatusColor(ticket.Status),
                    FontSize = 12
                });

                stack.Children.Add(header);

                stack.Children.Add(new TextBlock
                {
                    Text = ticket.Subject,
                    Foreground = Brushes.White,
                    Margin = new Thickness(0, 4, 0, 2)
                });

                stack.Children.Add(new TextBlock
                {
                    Text = $"By {ticket.Username} • {GetTimeAgo(ticket.CreatedAt)}",
                    Foreground = Brushes.Gray,
                    FontSize = 11
                });

                activityItem.Child = stack;
                ActivityPanel.Children.Add(activityItem);
            }
        }

        private Brush GetStatusColor(string status)
        {
            switch (status)
            {
                case "Open":
                    return new SolidColorBrush(Color.FromRgb(52, 152, 219));
                case "In Progress":
                    return new SolidColorBrush(Color.FromRgb(241, 196, 15));
                case "Resolved":
                    return new SolidColorBrush(Color.FromRgb(46, 204, 113));
                case "Closed":
                    return new SolidColorBrush(Color.FromRgb(149, 165, 166));
                default:
                    return Brushes.Gray;
            }
        }

        private string GetTimeAgo(DateTime dateTime)
        {
            var timeSpan = DateTime.Now - dateTime;

            if (timeSpan.TotalMinutes < 1)
                return "just now";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes}m ago";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours}h ago";
            if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays}d ago";

            return dateTime.ToString("dd/MM/yyyy");
        }

        private void ManageTickets_Click(object sender, MouseButtonEventArgs e)
        {
            refreshTimer.Stop();
            page.Navigate(new AdminTicketManagement());
        }

        private void ManageUsers_Click(object sender, MouseButtonEventArgs e)
        {
            refreshTimer.Stop();
            page.Navigate(new AdminUserManagement());
        }

        private void ViewReports_Click(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Reports feature coming soon!", "Info",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            refreshTimer.Stop();
            page.Navigate(new TeacherUI());
        }
    }
}