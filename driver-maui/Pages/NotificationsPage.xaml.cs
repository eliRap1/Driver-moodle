using driver_maui.Services;

namespace driver_maui.Pages
{
    public class NotifItem
    {
        public int NotificationId { get; set; }
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";
        public string TimeText { get; set; } = "";
        public Color BgColor { get; set; } = Colors.White;
    }

    public partial class NotificationsPage : ContentPage
    {
        public NotificationsPage() => InitializeComponent();

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadNotifications();
        }

        private async Task LoadNotifications()
        {
            try
            {
                var notifs = await ServiceHelper.CallAsync(srv =>
                    srv.GetUserNotificationsAsync(AppState.UserId, AppState.Role));

                NotifList.ItemsSource = notifs?
                    .OrderByDescending(n => n.CreatedAt)
                    .Select(n => new NotifItem
                    {
                        NotificationId = n.Id,
                        Title = n.Title ?? "",
                        Message = n.Message ?? "",
                        TimeText = n.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                        BgColor = n.IsRead ? Colors.White : Color.FromArgb("#E3F2FD")
                    }).ToList();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async void MarkAllRead_Click(object s, EventArgs e)
        {
            try
            {
                await ServiceHelper.CallAsync(srv =>
                    srv.MarkAllNotificationsAsReadAsync(AppState.UserId, AppState.Role));
                await LoadNotifications();
            }
            catch { }
        }

        private async void Back_Click(object s, EventArgs e) =>
            await Shell.Current.GoToAsync(AppState.Role == "Teacher" ? "//TeacherHome" : "//StudentHome");
    }
}
