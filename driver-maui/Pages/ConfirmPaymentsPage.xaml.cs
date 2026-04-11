using driver_maui.Services;

namespace driver_maui.Pages
{
    public class PendingLessonItem
    {
        public int LessonId { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = "";
        public string DateTimeText { get; set; } = "";
    }

    public partial class ConfirmPaymentsPage : ContentPage
    {
        public ConfirmPaymentsPage() => InitializeComponent();

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            ResultLabel.IsVisible = false;
            await LoadData();
        }

        private async Task LoadData()
        {
            try
            {
                var lessons = await ServiceHelper.CallAsync(srv => srv.GetAllTeacherLessonsAsync(AppState.UserId));
                var pending = lessons?.Where(l => !l.paid && l.Canceled != 1).ToList() ?? new();

                var displayList = new List<PendingLessonItem>();
                foreach (var l in pending)
                {
                    string studentName;
                    try
                    {
                        var student = await ServiceHelper.CallAsync(srv => srv.GetUserByIdAsync(l.StudentId, "Student"));
                        studentName = student?.Username ?? $"Student #{l.StudentId}";
                    }
                    catch { studentName = $"Student #{l.StudentId}"; }

                    displayList.Add(new PendingLessonItem
                    {
                        LessonId = l.LessonId,
                        StudentId = l.StudentId,
                        StudentName = studentName,
                        DateTimeText = $"{l.Date} {l.Time}"
                    });
                }
                LessonsList.ItemsSource = displayList;
            }
            catch (Exception ex)
            {
                ResultLabel.Text = $"Error: {ex.Message}";
                ResultLabel.TextColor = Colors.Red;
                ResultLabel.IsVisible = true;
            }
        }

        private async void ConfirmBtn_Click(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is int lessonId)
            {
                try
                {
                    await ServiceHelper.CallAsync(srv => srv.MarkLessonPaidAsync(lessonId));
                    ResultLabel.Text = "Payment confirmed!";
                    ResultLabel.TextColor = Colors.Green;
                    ResultLabel.IsVisible = true;
                    await LoadData();
                }
                catch (Exception ex)
                {
                    ResultLabel.Text = $"Error: {ex.Message}";
                    ResultLabel.TextColor = Colors.Red;
                    ResultLabel.IsVisible = true;
                }
            }
        }

        private async void Back_Click(object s, EventArgs e) => await Shell.Current.GoToAsync("//TeacherHome");
    }
}
