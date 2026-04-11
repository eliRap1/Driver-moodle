using driver_maui.Services;

namespace driver_maui.Pages
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage() => InitializeComponent();

        private async void LoginBtn_Clicked(object sender, EventArgs e)
        {
            ErrorLabel.IsVisible = false;
            string username = UsernameEntry.Text?.Trim() ?? "";
            string password = PasswordEntry.Text ?? "";
            string role = RolePicker.SelectedItem?.ToString() ?? "";

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(role))
            {
                ErrorLabel.Text = "Please fill all fields and select a role.";
                ErrorLabel.IsVisible = true;
                return;
            }

            try
            {
                bool valid = await ServiceHelper.CallAsync(srv => srv.CheckUserPasswordAsync(username, password));
                if (!valid)
                {
                    ErrorLabel.Text = "Invalid username or password.";
                    ErrorLabel.IsVisible = true;
                    return;
                }

                string table = role == "Teacher" ? "Teacher" : "Student";
                int userId = await ServiceHelper.CallAsync(srv => srv.GetUserIDAsync(username, table));
                bool isAdmin = await ServiceHelper.CallAsync(srv => srv.IsUserAdminAsync(username));

                AppState.Username = username;
                AppState.UserId = userId;
                AppState.Role = role;
                AppState.IsAdmin = isAdmin;

                if (role == "Student")
                    await Shell.Current.GoToAsync("//StudentHome");
                else
                    await Shell.Current.GoToAsync("//TeacherHome");
            }
            catch (Exception ex)
            {
                ErrorLabel.Text = $"Connection error: {ex.Message}";
                ErrorLabel.IsVisible = true;
            }
        }

        private async void SignUpBtn_Clicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//SignUp");
        }
    }
}
