using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using driver_client.driver;

namespace driver_client
{
    public partial class TeacherUI : Page
    {
        private bool isAdmin = false;
        private int teacherId;
        private DispatcherTimer notificationTimer;

        public TeacherUI()
        {
            InitializeComponent();
            teacherName.Text = LogIn.sign.Username;

            teacherId = ClientSession.TeacherId;

            // Check if user is admin using DATABASE
            CheckAdminStatus();

            if (isAdmin)
            {
                AdminBadge.Visibility = Visibility.Visible;
                AdminDashboardBtn.Visibility = Visibility.Visible;
            }

            // Load dashboard stats
            LoadDashboardStats();

            // Start notification timer
            notificationTimer = new DispatcherTimer();
            notificationTimer.Interval = TimeSpan.FromSeconds(30);
            notificationTimer.Tick += UpdateNotificationBadge;
            notificationTimer.Start();
            UpdateNotificationBadge(null, null);
        }

        private void CheckAdminStatus()
        {
            try
            {
                isAdmin = ServiceGateway.Use(client => client.IsUserAdmin(LogIn.sign.Username));
                System.Diagnostics.Debug.WriteLine($"Admin check for {LogIn.sign.Username}: {isAdmin}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Admin check error: {ex.Message}");
                isAdmin = false;
            }
        }

        private void LoadDashboardStats()
        {
            try
            {
                // Get students count
                var students = ServiceGateway.Use(client => client.GetTeacherStudents(teacherId));
                if (students != null)
                {
                    TotalStudentsText.Text = students.Length.ToString();
                }

                // Get today's lessons
                var lessons = ServiceGateway.Use(client => client.GetAllTeacherLessons(teacherId));
                if (lessons != null)
                {
                    var today = DateTime.Today.ToString("dd-MM-yyyy");
                    var todayAlt = DateTime.Today.ToString("dd/MM/yyyy");
                    var todayIso = DateTime.Today.ToString("yyyy-MM-dd");
                    int todayLessons = lessons.Count(l => l.Canceled != 1 &&
                        (l.Date == today || l.Date == todayAlt || l.Date == todayIso));
                    TodayLessonsText.Text = todayLessons.ToString();

                    // Get unpaid lessons count
                    int unpaidLessons = lessons.Count(l => l.Canceled != 1 && !l.paid);
                    PendingPaymentsText.Text = unpaidLessons.ToString();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadDashboardStats Error: {ex.Message}");
            }
        }

        private void UpdateNotificationBadge(object sender, EventArgs e)
        {
            try
            {
                int unreadCount = ServiceGateway.Use(client => client.GetUnreadNotificationCount(teacherId, "Teacher"));

                UnreadNotificationsText.Text = unreadCount.ToString();

                if (unreadCount > 0)
                {
                    NotificationBadge.Visibility = Visibility.Visible;
                    NotificationCount.Text = unreadCount > 99 ? "99+" : unreadCount.ToString();
                }
                else
                {
                    NotificationBadge.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateNotificationBadge Error: {ex.Message}");
                NotificationBadge.Visibility = Visibility.Collapsed;
            }
        }

        private void AdminDashboard_Click(object sender, RoutedEventArgs e)
        {
            page.Navigate(new AdminDashboard());
        }

        private void Students_Click(object sender, RoutedEventArgs e)
        {
            page.Navigate(new AllStudents(teacherId));
        }

        private void Calendar_Click(object sender, RoutedEventArgs e)
        {
            CalendarTeacher calendar = new CalendarTeacher(LogIn.sign);
            page.Navigate(calendar);
        }

        private void Schedule_Click(object sender, RoutedEventArgs e)
        {
            page.Navigate(new Teacher_Schedule());
        }

        private void PaymentReports_Click(object sender, RoutedEventArgs e)
        {
            page.Navigate(new TeacherPaymentReports());
        }

        private void ConfirmPayments_Click(object sender, RoutedEventArgs e)
        {
            page.Navigate(new TeacherConfirmPayments());
        }

        private void Notifications_Click(object sender, RoutedEventArgs e)
        {
            page.Navigate(new TeacherNotifications());
        }

        private void Chat_Click(object sender, RoutedEventArgs e)
        {
            page.Navigate(new Chat());
        }

        private void SupportTickets_Click(object sender, RoutedEventArgs e)
        {
            page.Navigate(new MyTickets());
        }

        private void ManageCourses_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Course management is coming soon.", "Coming Soon",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            page.Navigate(new TeacherSettings());
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to logout?", "Confirm Logout",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Clear login data
                LogIn.sign = new Sign();
                notificationTimer.Stop();
                page.Navigate(new LogIn());
            }
        }
    }
}
