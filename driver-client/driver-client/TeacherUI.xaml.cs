using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace driver_client
{
    /// <summary>
    /// Interaction logic for TeacherUI.xaml
    /// </summary>
    public partial class TeacherUI : Page
    {
        private bool isAdmin = false;

        public TeacherUI()
        {
            InitializeComponent();
            teacherName.Text = LogIn.sign.Username;

            // Check if user is admin
            isAdmin = IsUserAdmin(LogIn.sign.Username);

            if (isAdmin)
            {
                AdminBadge.Visibility = Visibility.Visible;
                AdminDashboardBtn.Visibility = Visibility.Visible;
            }
        }

        private bool IsUserAdmin(string username)
        {
            if (string.IsNullOrEmpty(username))
                return false;

            // Admin usernames - should match server-side logic
            string[] adminUsers = { "admin", "Admin", "ADMIN" };
            return adminUsers.Contains(username);
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
