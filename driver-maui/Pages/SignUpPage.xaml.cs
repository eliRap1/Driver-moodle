using driver_maui.Services;

namespace driver_maui.Pages
{
    public partial class SignUpPage : ContentPage
    {
        public SignUpPage()
        {
            InitializeComponent();
            RolePicker.SelectedIndexChanged += (s, e) =>
                TeacherFields.IsVisible = RolePicker.SelectedItem?.ToString() == "Teacher";
        }

        private async void Register_Click(object sender, EventArgs e)
        {
            ErrorLabel.IsVisible = false;
            string username = UsernameEntry.Text?.Trim() ?? "";
            string password = PasswordEntry.Text ?? "";
            string confirm = ConfirmEntry.Text ?? "";
            string email = EmailEntry.Text?.Trim() ?? "";
            string phone = PhoneEntry.Text?.Trim() ?? "";
            string role = RolePicker.SelectedItem?.ToString() ?? "";

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) ||
                string.IsNullOrEmpty(email) || string.IsNullOrEmpty(role))
            {
                ErrorLabel.Text = "Please fill all required fields.";
                ErrorLabel.IsVisible = true;
                return;
            }

            if (password != confirm)
            {
                ErrorLabel.Text = "Passwords do not match.";
                ErrorLabel.IsVisible = true;
                return;
            }

            bool isTeacher = role == "Teacher";
            int lessonPrice = 200;
            if (isTeacher && int.TryParse(LessonPriceEntry.Text, out int p) && p > 0)
                lessonPrice = p;

            try
            {
                bool exists = await ServiceHelper.CallAsync(srv => srv.CheckUserExistAsync(username));
                if (exists)
                {
                    ErrorLabel.Text = "Username already taken. Please choose another.";
                    ErrorLabel.IsVisible = true;
                    return;
                }

                bool success = await ServiceHelper.CallAsync(srv =>
                    srv.AddUserAsync(username, password, email, phone, isTeacher, 0, lessonPrice));

                if (success)
                {
                    await DisplayAlert("Success", "Account created! Please log in.", "OK");
                    await Shell.Current.GoToAsync("//Login");
                }
                else
                {
                    ErrorLabel.Text = "Registration failed. Please try again.";
                    ErrorLabel.IsVisible = true;
                }
            }
            catch (Exception ex)
            {
                ErrorLabel.Text = $"Error: {ex.Message}";
                ErrorLabel.IsVisible = true;
            }
        }

        private async void Back_Click(object s, EventArgs e) => await Shell.Current.GoToAsync("//Login");
    }
}
