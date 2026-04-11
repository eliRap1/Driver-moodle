using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using webTOsrv;

namespace Driver.Pages.Teacher
{
    public class NotificationsModel : PageModel
    {
        private Service1Client srv = new Service1Client();

        public List<Notification> Notifications { get; set; } = new();
        public List<UserInfo> Students { get; set; } = new();
        public int UnreadCount { get; set; }
        public int CancelledLessonsCount { get; set; }
        public int PaymentNotificationsCount { get; set; }
        public string Message { get; set; } = "";
        public bool IsSuccess { get; set; }

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

        public IActionResult OnPostSendMessage(int studentId, string title, string message)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var username = HttpContext.Session.GetString("Username");
            if (userId == null) return RedirectToPage("/Login");

            try
            {
                srv.SendTeacherMessage(userId.Value, username ?? "Teacher", studentId, title, message);
                Message = "Message sent successfully!";
                IsSuccess = true;
            }
            catch (Exception ex)
            {
                Message = $"Error sending message: {ex.Message}";
                IsSuccess = false;
            }

            LoadData(userId.Value);
            return Page();
        }

        public IActionResult OnPostMarkRead(int notificationId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToPage("/Login");

            try
            {
                srv.MarkNotificationAsRead(notificationId);
            }
            catch { }

            LoadData(userId.Value);
            return Page();
        }

        public IActionResult OnPostMarkAllRead()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToPage("/Login");

            try
            {
                srv.MarkAllNotificationsAsRead(userId.Value, "Teacher");
                Message = "All notifications marked as read.";
                IsSuccess = true;
            }
            catch (Exception ex)
            {
                Message = $"Error: {ex.Message}";
                IsSuccess = false;
            }

            LoadData(userId.Value);
            return Page();
        }

        public IActionResult OnPostDelete(int notificationId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToPage("/Login");

            try
            {
                srv.DeleteNotification(notificationId);
            }
            catch { }

            LoadData(userId.Value);
            return Page();
        }

        private void LoadData(int teacherId)
        {
            try
            {
                // Load notifications
                Notifications = srv.GetUserNotifications(teacherId, "Teacher").ToList();
                UnreadCount = srv.GetUnreadNotificationCount(teacherId, "Teacher");

                // Count by type
                CancelledLessonsCount = Notifications.Count(n => n.NotificationType == "LessonCancelled");
                PaymentNotificationsCount = Notifications.Count(n => n.NotificationType == "PaymentReceived");

                // Load students for sending messages
                var students = srv.GetTeacherStudents(teacherId);
                if (students != null)
                {
                    Students = students.ToList();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading data: {ex.Message}");
            }
        }
    }
}
