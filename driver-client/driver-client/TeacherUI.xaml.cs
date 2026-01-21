// FIXED TeacherUI.xaml.cs
// Replace your existing file with this version

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using driver_client.driver;

namespace driver_client
{
    public partial class TeacherUI : Page
    {
        private bool isAdmin = false;

        public TeacherUI()
        {
            InitializeComponent();
            teacherName.Text = LogIn.sign.Username;

            // Check if user is admin using DATABASE, not hardcoded names
            CheckAdminStatus();

            if (isAdmin)
            {
                AdminBadge.Visibility = Visibility.Visible;
                AdminDashboardBtn.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// FIXED: Check admin status from database instead of hardcoded usernames
        /// </summary>
        private void CheckAdminStatus()
        {
            try
            {
                var srv = new Service1Client();
                isAdmin = srv.IsUserAdmin(LogIn.sign.Username);
                System.Diagnostics.Debug.WriteLine($"Admin check for {LogIn.sign.Username}: {isAdmin}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Admin check error: {ex.Message}");
                isAdmin = false;
            }
        }

        private void AdminDashboard_Click(object sender, RoutedEventArgs e)
        {
            page.Navigate(new AdminDashboard());
        }

        private void Students_Click(object sender, RoutedEventArgs e)
        {
            page.Navigate(new AllStudents(LogIn.sign.Id));
        }

        private void Calendar_Click(object sender, RoutedEventArgs e)
        {
            CalendarTeacher calendar = new CalendarTeacher(LogIn.sign);
            page.Navigate(calendar);
        }

        private void TestSchedule_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement test schedule
        }

        private void Schedule_Click(object sender, RoutedEventArgs e)
        {
            page.Navigate(new Teacher_Schedule());
        }

        private void PaymentReports_Click(object sender, RoutedEventArgs e)
        {
            page.Navigate(new TeacherPaymentReports());
        }

        private void Chat_Click(object sender, RoutedEventArgs e)
        {
            page.Navigate(new Chat());
        }

        private void SupportTickets_Click(object sender, RoutedEventArgs e)
        {
            page.Navigate(new MyTickets());
        }

        // NEW: Settings button handler
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
                page.Navigate(new LogIn());
            }
        }
    }
}