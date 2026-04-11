using driver_maui.Services;

namespace driver_maui.Pages
{
    public partial class StudentHomePage : ContentPage
    {
        public StudentHomePage() => InitializeComponent();

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            WelcomeLabel.Text = $"Welcome, {AppState.Username}!";
            await LoadStats();
        }

        private async Task LoadStats()
        {
            try
            {
                var lessons = await ServiceHelper.CallAsync(srv => srv.GetAllStudentLessonsAsync(AppState.UserId));
                if (lessons != null)
                {
                    TotalLessonsLabel.Text = lessons.Count(l => l.Canceled != 1).ToString();
                    UnpaidLabel.Text = lessons.Count(l => l.Canceled != 1 && !l.paid).ToString();
                }
            }
            catch (Exception ex)
            {
                StatusLabel.Text = $"Could not load stats: {ex.Message}";
                StatusLabel.IsVisible = true;
            }
        }

        private async void ViewLessons_Click(object s, EventArgs e) => await Shell.Current.GoToAsync("//ViewLessons");
        private async void Payments_Click(object s, EventArgs e) => await Shell.Current.GoToAsync("//Payments");
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
