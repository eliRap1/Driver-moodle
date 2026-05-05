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
using System.Windows.Threading;

namespace driver_client
{
    /// <summary>
    /// Interaction logic for StudentUI.xaml
    /// </summary>
    public partial class StudentUI : Page
    {
        private int id;
        private DispatcherTimer updateAprove;
        private DispatcherTimer notificationTimer;
        public static bool madeRewiew = false;

        public StudentUI()
        {
            InitializeComponent();
            id = ClientSession.StudentId;

            WelcomeText.Text = LogIn.sign.Username;

            updateAprove = new DispatcherTimer();
            updateAprove.Interval = TimeSpan.FromSeconds(5);
            updateAprove.Tick += CheckIfApproved;
            updateAprove.Start();
            CheckIfApproved(null, null);

            // Start notification timer
            notificationTimer = new DispatcherTimer();
            notificationTimer.Interval = TimeSpan.FromSeconds(30);
            notificationTimer.Tick += UpdateNotificationBadge;
            notificationTimer.Start();

            if (madeRewiew)
            {
                writeReview.IsEnabled = false;
            }
        }

        private void CheckIfApproved(object sender, EventArgs e)
        {
            try
            {
                var student = ServiceGateway.Use(client => client.GetUserById(id, "Student"));
                if (student != null && student.Confirmed == true)
                {
                    WaitingPanel.Visibility = Visibility.Collapsed;
                    StudentPanel.Visibility = Visibility.Visible;
                    updateAprove.Stop();

                    // Load dashboard stats
                    LoadDashboardStats();
                    UpdateNotificationBadge(null, null);
                }
                else
                {
                    WaitingPanel.Visibility = Visibility.Visible;
                    StudentPanel.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CheckIfApproved Error: {ex.Message}");
            }
        }

        private void LoadDashboardStats()
        {
            try
            {
                // Get lessons stats
                var lessons = ServiceGateway.Use(client => client.GetAllStudentLessons(id));
                if (lessons != null)
                {
                    int totalLessons = lessons.Count(l => l.Canceled != 1);
                    int unpaidLessons = lessons.Count(l => l.Canceled != 1 && !l.paid);

                    TotalLessonsText.Text = totalLessons.ToString();
                    UnpaidLessonsText.Text = unpaidLessons.ToString();
                }

                CourseProgressText.Text = "Soon";
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
                int unreadCount = ServiceGateway.Use(client => client.GetUnreadNotificationCount(id, "Student"));

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

        private void ScheduleLesson_Click(object sender, RoutedEventArgs e)
        {
            page.Navigate(new ScheduleLesson());
        }

        private void WriteReview_Click(object sender, RoutedEventArgs e)
        {
            page.Navigate(new WriteRewiew());
        }

        private void ViewLessons_Click(object sender, RoutedEventArgs e)
        {
            page.Navigate(new ViewLessons());
        }

        private void Payments_Click(object sender, RoutedEventArgs e)
        {
            page.Navigate(new StudentPayment());
        }

        private void Courses_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Courses are coming soon.", "Coming Soon",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Notifications_Click(object sender, RoutedEventArgs e)
        {
            page.Navigate(new StudentNotifications());
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to logout?", "Confirm Logout",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Clear login data
                LogIn.sign = new Sign();
                updateAprove.Stop();
                notificationTimer.Stop();
                page.Navigate(new LogIn());
            }
        }

        private void Review_Click(object sender, RoutedEventArgs e)
        {
            page.Navigate(new ChooseTeacher(false));
        }

        private void Chat_Click(object sender, RoutedEventArgs e)
        {
            page.Navigate(new Chat());
        }

        private void SupportTickets_Click(object sender, RoutedEventArgs e)
        {
            page.Navigate(new MyTickets());
        }
    }
}
