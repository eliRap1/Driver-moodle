using driver_maui.Services;
using DriverService;
using System.Text.RegularExpressions;

namespace driver_maui.Pages
{
    public partial class SignUpPage : ContentPage
    {
        private List<UserInfo> teachers = new();
        private static readonly Regex EmailRx = new(@"^[\w\.\-]+@[\w\-]+\.[\w\-\.]+$", RegexOptions.Compiled);
        private static readonly Regex PhoneRx = new(@"^\+?\d{7,15}$", RegexOptions.Compiled);

        public SignUpPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadTeachers();
        }

        private async Task LoadTeachers()
        {
            try
            {
                var all = await ServiceHelper.CallAsync(srv => srv.GetAllTeacherAsync());
                teachers = all?.ToList() ?? new();
                TeacherPicker.ItemsSource = teachers
                    .Select(t => $"{t.Username} (#{t.Id}, ₪{t.LessonPrice}/lesson)")
                    .ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadTeachers: {ex}");
                ErrorLabel.Text = "Could not load teacher list.";
                ErrorLabel.IsVisible = true;
            }
        }

        private async void Register_Click(object sender, EventArgs e)
        {
            ErrorLabel.IsVisible = false;
            string username = UsernameEntry.Text?.Trim() ?? "";
            string password = PasswordEntry.Text ?? "";
            string confirm = ConfirmEntry.Text ?? "";
            string email = EmailEntry.Text?.Trim() ?? "";
            string phone = PhoneEntry.Text?.Trim() ?? "";

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) ||
                string.IsNullOrEmpty(email) || string.IsNullOrEmpty(phone))
            {
                ShowError("Please fill all required fields.");
                return;
            }
            if (username.Length < 4 || password.Length < 4)
            {
                ShowError("Username and password must be at least 4 characters.");
                return;
            }
            if (password != confirm)
            {
                ShowError("Passwords do not match.");
                return;
            }
            if (!EmailRx.IsMatch(email))
            {
                ShowError("Please enter a valid email address.");
                return;
            }
            if (!PhoneRx.IsMatch(phone))
            {
                ShowError("Please enter a valid phone number.");
                return;
            }
            if (TeacherPicker.SelectedIndex < 0 || TeacherPicker.SelectedIndex >= teachers.Count)
            {
                ShowError("Please choose a teacher.");
                return;
            }

            int teacherId = teachers[TeacherPicker.SelectedIndex].Id;

            try
            {
                bool exists = await ServiceHelper.CallAsync(srv => srv.CheckUserExistAsync(username));
                if (exists)
                {
                    ShowError("Username already taken. Please choose another.");
                    return;
                }

                bool success = await ServiceHelper.CallAsync(srv =>
                    srv.AddUserAsync(username, password, email, phone, /*admin*/ false, teacherId, 0));

                if (success)
                {
                    await DisplayAlert("Success", "Account created! Please log in.", "OK");
                    await Shell.Current.GoToAsync("//Login");
                }
                else
                {
                    ShowError("Registration failed. Please try again.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SignUp: {ex}");
                ShowError("Service error. Please try again.");
            }
        }

        private void ShowError(string text)
        {
            ErrorLabel.Text = text;
            ErrorLabel.IsVisible = true;
        }

        private async void Back_Click(object s, EventArgs e) => await Shell.Current.GoToAsync("//Login");
    }
}
