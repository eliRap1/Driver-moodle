using driver_maui.Services;
using DriverService;

namespace driver_maui.Pages
{
    public class NotifItem
    {
        public int NotificationId { get; set; }
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";
        public string TimeText { get; set; } = "";
        public Color BgColor { get; set; } = Colors.White;
        public bool ShowMarkRead { get; set; }
    }

    public class RecipientOption
    {
        public int Id { get; set; }
        public string DisplayName { get; set; } = "";
        public override string ToString() => DisplayName;
    }

    public partial class NotificationsPage : ContentPage
    {
        private List<RecipientOption> _recipients = new();

        public NotificationsPage() => InitializeComponent();

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadRecipients();
            await LoadNotifications();
        }

        private async Task LoadRecipients()
        {
            try
            {
                _recipients = new List<RecipientOption>();
                if (AppState.Role == "Teacher")
                {
                    var students = await ServiceHelper.CallAsync(srv => srv.GetTeacherStudentsAsync(AppState.UserId));
                    if (students != null)
                    {
                        foreach (var s in students)
                        {
                            int sid = s.StudentId > 0 ? s.StudentId : s.Id;
                            _recipients.Add(new RecipientOption
                            {
                                Id = sid,
                                DisplayName = s.Username ?? $"Student #{sid}"
                            });
                        }
                    }
                }
                else if (AppState.Role == "Student")
                {
                    int teacherId = await ServiceHelper.CallAsync(srv => srv.GetTeacherIdAsync(AppState.UserId));
                    if (teacherId > 0)
                    {
                        var teacher = await ServiceHelper.CallAsync(srv => srv.GetUserByIdAsync(teacherId, "Teacher"));
                        _recipients.Add(new RecipientOption
                        {
                            Id = teacherId,
                            DisplayName = teacher?.Username ?? $"Teacher #{teacherId}"
                        });
                    }
                }

                RecipientPicker.ItemsSource = _recipients;
                if (_recipients.Count > 0) RecipientPicker.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadRecipients: {ex}");
            }
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
                        Message = $"{(string.IsNullOrEmpty(n.SenderName) ? "" : $"From {n.SenderName}: ")}{n.Message}",
                        TimeText = n.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                        BgColor = n.IsRead ? Colors.White : Color.FromArgb("#E3F2FD"),
                        ShowMarkRead = !n.IsRead
                    }).ToList();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async void Send_Click(object sender, EventArgs e)
        {
            SendStatusLabel.IsVisible = false;
            string title = TitleEntry.Text?.Trim() ?? "";
            string message = MessageEditor.Text?.Trim() ?? "";

            if (RecipientPicker.SelectedItem is not RecipientOption recipient)
            {
                ShowSendStatus("Pick a recipient.", Colors.Red);
                return;
            }
            if (string.IsNullOrEmpty(title))
            {
                ShowSendStatus("Title required.", Colors.Red);
                return;
            }
            if (string.IsNullOrEmpty(message))
            {
                ShowSendStatus("Message required.", Colors.Red);
                return;
            }

            try
            {
                if (AppState.Role == "Teacher")
                {
                    await ServiceHelper.CallAsync(srv => srv.SendTeacherMessageAsync(
                        AppState.UserId, AppState.Username, recipient.Id, title, message));
                }
                else
                {
                    await ServiceHelper.CallAsync(srv => srv.SendStudentMessageAsync(
                        AppState.UserId, AppState.Username, recipient.Id, title, message));
                }
                TitleEntry.Text = "";
                MessageEditor.Text = "";
                ShowSendStatus("Sent.", Colors.Green);
                await LoadNotifications();
            }
            catch (Exception ex)
            {
                ShowSendStatus($"Send failed: {ex.Message}", Colors.Red);
            }
        }

        private void ShowSendStatus(string text, Color color)
        {
            SendStatusLabel.Text = text;
            SendStatusLabel.TextColor = color;
            SendStatusLabel.IsVisible = true;
        }

        private async void MarkRead_Click(object sender, EventArgs e)
        {
            if (sender is Button b && b.CommandParameter is int id)
            {
                try
                {
                    await ServiceHelper.CallAsync(srv => srv.MarkNotificationAsReadAsync(id));
                    await LoadNotifications();
                }
                catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"MarkRead: {ex}"); }
            }
        }

        private async void Delete_Click(object sender, EventArgs e)
        {
            if (sender is Button b && b.CommandParameter is int id)
            {
                bool ok = await DisplayAlert("Delete", "Delete this notification?", "Yes", "No");
                if (!ok) return;
                try
                {
                    await ServiceHelper.CallAsync(srv => srv.DeleteNotificationAsync(id));
                    await LoadNotifications();
                }
                catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Delete: {ex}"); }
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
