using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using driver_client.driver;

namespace driver_client
{
    public partial class TeacherNotifications : Page
    {
        private int teacherId;

        public TeacherNotifications()
        {
            InitializeComponent();
            teacherId = ClientSession.TeacherId;

            LoadStudents();
            LoadNotifications();
        }

        private void LoadStudents()
        {
            try
            {
                var srv = new Service1Client();
                var students = srv.GetTeacherStudents(teacherId);
                if (students != null)
                {
                    StudentComboBox.ItemsSource = students;
                    StudentComboBox.SelectedIndex = students.Length > 0 ? 0 : -1;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadStudents Error: {ex.Message}");
            }
        }

        private void LoadNotifications()
        {
            try
            {
                var srv = new Service1Client();
                var notifications = srv.GetUserNotifications(teacherId, "Teacher");

                if (notifications != null && notifications.Length > 0)
                {
                    var viewModels = notifications.Select(n => new TeacherNotificationViewModel
                    {
                        Id = n.Id,
                        Title = n.Title ?? "Notification",
                        Message = n.Message ?? "",
                        SenderName = n.SenderName ?? "System",
                        NotificationType = n.NotificationType ?? "Message",
                        IsRead = n.IsRead,
                        CreatedAt = n.CreatedAt
                    }).ToList();

                    NotificationsList.ItemsSource = viewModels;
                    EmptyState.Visibility = Visibility.Collapsed;

                    // Update stats
                    UnreadCountText.Text = viewModels.Count(n => !n.IsRead).ToString();
                    CancelledCountText.Text = viewModels.Count(n => n.NotificationType == "LessonCancelled").ToString();
                    PaymentCountText.Text = viewModels.Count(n => n.NotificationType == "PaymentReceived").ToString();
                    TotalCountText.Text = viewModels.Count.ToString();
                }
                else
                {
                    NotificationsList.ItemsSource = null;
                    EmptyState.Visibility = Visibility.Visible;
                    UnreadCountText.Text = "0";
                    CancelledCountText.Text = "0";
                    PaymentCountText.Text = "0";
                    TotalCountText.Text = "0";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading notifications: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedStudent = StudentComboBox.SelectedItem as UserInfo;
                if (selectedStudent == null)
                {
                    MessageBox.Show("Please select a student.", "Validation Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string title = TitleTextBox.Text?.Trim();
                string message = MessageTextBox.Text?.Trim();

                if (string.IsNullOrEmpty(title))
                {
                    MessageBox.Show("Please enter a title.", "Validation Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(message))
                {
                    MessageBox.Show("Please enter a message.", "Validation Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var srv = new Service1Client();
                int studentId = selectedStudent.StudentId > 0 ? selectedStudent.StudentId : selectedStudent.Id;
                srv.SendTeacherMessage(teacherId, LogIn.sign.Username, studentId, title, message);

                MessageBox.Show("Message sent successfully!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                // Clear form
                TitleTextBox.Clear();
                MessageTextBox.Clear();
                LoadNotifications();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending message: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MarkRead_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                if (button?.Tag != null)
                {
                    int notificationId = Convert.ToInt32(button.Tag);
                    var srv = new Service1Client();
                    srv.MarkNotificationAsRead(notificationId);
                    LoadNotifications();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error marking notification as read: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MarkAllRead_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var srv = new Service1Client();
                srv.MarkAllNotificationsAsRead(teacherId, "Teacher");
                LoadNotifications();
                MessageBox.Show("All notifications marked as read.", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error marking all as read: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                if (button?.Tag != null)
                {
                    var result = MessageBox.Show("Delete this notification?", "Confirm Delete",
                        MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        int notificationId = Convert.ToInt32(button.Tag);
                        var srv = new Service1Client();
                        srv.DeleteNotification(notificationId);
                        LoadNotifications();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting notification: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadNotifications();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            page.Navigate(new TeacherUI());
        }
    }

    public class TeacherNotificationViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string SenderName { get; set; }
        public string NotificationType { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }

        public string FormattedDate => CreatedAt.ToString("dd/MM/yyyy HH:mm");

        public string TypeIcon
        {
            get
            {
                switch (NotificationType)
                {
                    case "LessonCancelled":
                        return "X";
                    case "PaymentReceived":
                        return "$";
                    case "Reminder":
                        return "!";
                    default:
                        return "M";
                }
            }
        }

        public Brush TypeColor
        {
            get
            {
                switch (NotificationType)
                {
                    case "LessonCancelled":
                        return new SolidColorBrush(Color.FromRgb(231, 76, 60));
                    case "PaymentReceived":
                        return new SolidColorBrush(Color.FromRgb(39, 174, 96));
                    case "Reminder":
                        return new SolidColorBrush(Color.FromRgb(243, 156, 18));
                    default:
                        return new SolidColorBrush(Color.FromRgb(0, 224, 255));
                }
            }
        }

        public Brush BackgroundColor
        {
            get
            {
                return IsRead
                    ? new SolidColorBrush(Color.FromRgb(14, 26, 43))
                    : new SolidColorBrush(Color.FromRgb(26, 38, 54));
            }
        }

        public Visibility UnreadBadgeVisibility => IsRead ? Visibility.Collapsed : Visibility.Visible;
        public Visibility MarkReadVisibility => IsRead ? Visibility.Collapsed : Visibility.Visible;
    }
}
