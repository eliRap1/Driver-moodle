using driver_maui.Services;

namespace driver_maui.Pages
{
    public class ChatItem
    {
        public string Username { get; set; } = "";
        public string Message { get; set; } = "";
        public bool IsMe { get; set; }
        public LayoutOptions Alignment => IsMe ? LayoutOptions.End : LayoutOptions.Start;
        public Color BgColor => IsMe ? Color.FromArgb("#DCF8C6") : Color.FromArgb("#F0F0F0");
        public Color NameColor => IsMe ? Colors.DarkGreen : Colors.DarkBlue;
    }

    public partial class ChatPage : ContentPage
    {
        public ChatPage() => InitializeComponent();

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadMessages();
        }

        private async Task LoadMessages()
        {
            try
            {
                var messages = await ServiceHelper.CallAsync(srv => srv.GetAllChatGlobalAsync());
                MessagesList.ItemsSource = messages?
                    .OrderBy(m => m.Id)
                    .Select(m => new ChatItem
                    {
                        Username = m.Username ?? "",
                        Message = m.Message ?? "",
                        IsMe = m.Username == AppState.Username
                    }).ToList();
            }
            catch { }
        }

        private async void Send_Click(object sender, EventArgs e)
        {
            string msg = MessageEntry.Text?.Trim() ?? "";
            if (string.IsNullOrEmpty(msg)) return;

            try
            {
                bool isTeacher = AppState.Role == "Teacher";
                await ServiceHelper.CallAsync(srv =>
                    srv.AddMessageGlobalAsync(msg, AppState.UserId, AppState.Username, isTeacher));
                MessageEntry.Text = "";
                await LoadMessages();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async void Back_Click(object s, EventArgs e) =>
            await Shell.Current.GoToAsync(AppState.Role == "Teacher" ? "//TeacherHome" : "//StudentHome");
    }
}
