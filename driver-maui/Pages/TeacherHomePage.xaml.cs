using driver_maui.Services;

namespace driver_maui.Pages
{
    public partial class TeacherHomePage : ContentPage
    {
        public TeacherHomePage() => InitializeComponent();

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (!await AppState.RequireRoleAsync(this, "Teacher")) return;
            WelcomeLabel.Text = $"Welcome, {AppState.Username}!";
            AdminBadge.IsVisible = AppState.IsAdmin;
            await LoadStats();
        }

        private async Task LoadStats()
        {
            try
            {
                var students = await ServiceHelper.CallAsync(srv => srv.GetTeacherStudentsAsync(AppState.UserId));
                StudentsCountLabel.Text = (students?.Length ?? 0).ToString();

                var lessons = await ServiceHelper.CallAsync(srv => srv.GetAllTeacherLessonsAsync(AppState.UserId));
                if (lessons != null)
                {
                    string today = DateTime.Today.ToString("dd-MM-yyyy");
                    string todayAlt = DateTime.Today.ToString("dd/MM/yyyy");
                    string todayIso = DateTime.Today.ToString("yyyy-MM-dd");
                    TodayLessonsLabel.Text = lessons
                        .Count(l => l.Canceled != 1 &&
                            (l.Date == today || l.Date == todayAlt || l.Date == todayIso))
                        .ToString();
                    UnpaidCountLabel.Text = lessons
                        .Count(l => l.Canceled != 1 && !l.paid)
                        .ToString();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TeacherHome LoadStats: {ex.Message}");
                StatusLabel.Text = "Could not load stats.";
                StatusLabel.IsVisible = true;
            }
        }

        private async void ConfirmPayments_Click(object s, EventArgs e) => await Shell.Current.GoToAsync("//ConfirmPayments");
        private async void PaymentReports_Click(object s, EventArgs e) => await Shell.Current.GoToAsync("//PaymentReports");
        private async void Notifications_Click(object s, EventArgs e) => await Shell.Current.GoToAsync("//Notifications");
        private async void Chat_Click(object s, EventArgs e) => await Shell.Current.GoToAsync("//Chat");

        private async void Logout_Click(object s, EventArgs e)
        {
            bool confirm = await DisplayAlert("Logout", "Are you sure you want to logout?", "Yes", "No");
            if (confirm)
            {
                AppState.Clear();
                await Shell.Current.GoToAsync("//Login");
            }
        }
    }
}
