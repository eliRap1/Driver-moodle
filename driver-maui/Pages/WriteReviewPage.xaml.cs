using driver_maui.Services;

namespace driver_maui.Pages
{
    public partial class WriteReviewPage : ContentPage
    {
        private int _teacherId;

        public WriteReviewPage() => InitializeComponent();

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (!await AppState.RequireRoleAsync(this, "Student")) return;
            await LoadTeacher();
        }

        private async Task LoadTeacher()
        {
            try
            {
                _teacherId = await ServiceHelper.CallAsync(srv => srv.GetTeacherIdAsync(AppState.UserId));
                if (_teacherId > 0)
                {
                    var teacher = await ServiceHelper.CallAsync(srv => srv.GetUserByIdAsync(_teacherId, "Teacher"));
                    TeacherLabel.Text = teacher != null ? $"Teacher: {teacher.Username}" : "Teacher: Unknown";
                }
                else
                {
                    TeacherLabel.Text = "No teacher assigned.";
                }
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"LoadTeacher: {ex}"); }
        }

        private async void Submit_Click(object sender, EventArgs e)
        {
            ResultLabel.IsVisible = false;
            int idx = RatingPicker.SelectedIndex;
            if (idx < 0)
            {
                ShowResult("Pick a rating.", Colors.Red);
                return;
            }
            int rating = 5 - idx; // index 0 = 5, 1 = 4, ...

            string review = ReviewEditor.Text?.Trim() ?? "";
            if (string.IsNullOrEmpty(review))
            {
                ShowResult("Write something before submitting.", Colors.Red);
                return;
            }

            if (_teacherId <= 0)
            {
                ShowResult("No teacher assigned.", Colors.Red);
                return;
            }

            try
            {
                await ServiceHelper.CallAsync(srv => srv.UpdateRatingAsync(_teacherId, rating, review));
                ShowResult("Review submitted. Thanks!", Colors.Green);
                ReviewEditor.Text = "";
                RatingPicker.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                ShowResult($"Submit failed: {ex.Message}", Colors.Red);
            }
        }

        private void ShowResult(string text, Color color)
        {
            ResultLabel.Text = text;
            ResultLabel.TextColor = color;
            ResultLabel.IsVisible = true;
        }

        private async void Back_Click(object s, EventArgs e) => await Shell.Current.GoToAsync("//StudentHome");
    }
}
