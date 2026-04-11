using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using webTOsrv;

namespace Driver.Pages.Student
{
    public class NotificationsModel : PageModel
    {
        private Service1Client srv = new Service1Client();

        public List<Notification> Notifications { get; set; } = new();
        public int UnreadCount { get; set; }
        public string Message { get; set; } = "";
        public bool IsSuccess { get; set; }

        public IActionResult OnGet()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Student")
            {
                return RedirectToPage("/Login");
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Login");
            }

            LoadNotifications(userId.Value);
            return Page();
        }

        public IActionResult OnPostMarkRead(int notificationId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToPage("/Login");

            try
            {
                srv.MarkNotificationAsRead(notificationId);
                Message = "Notification marked as read.";
                IsSuccess = true;
            }
            catch (Exception ex)
            {
                Message = $"Error: {ex.Message}";
                IsSuccess = false;
            }

            LoadNotifications(userId.Value);
            return Page();
        }

        public IActionResult OnPostMarkAllRead()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToPage("/Login");

            try
            {
                srv.MarkAllNotificationsAsRead(userId.Value, "Student");
                Message = "All notifications marked as read.";
                IsSuccess = true;
            }
            catch (Exception ex)
            {
                Message = $"Error: {ex.Message}";
                IsSuccess = false;
            }

            LoadNotifications(userId.Value);
            return Page();
        }

        public IActionResult OnPostDelete(int notificationId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToPage("/Login");

            try
            {
                srv.DeleteNotification(notificationId);
                Message = "Notification deleted.";
                IsSuccess = true;
            }
            catch (Exception ex)
            {
                Message = $"Error: {ex.Message}";
                IsSuccess = false;
            }

            LoadNotifications(userId.Value);
            return Page();
        }

        private void LoadNotifications(int userId)
        {
            try
            {
                Notifications = srv.GetUserNotifications(userId, "Student").ToList();
                UnreadCount = srv.GetUnreadNotificationCount(userId, "Student");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading notifications: {ex.Message}");
            }
        }
    }
}
