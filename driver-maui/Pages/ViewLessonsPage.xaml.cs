using driver_maui.Services;

namespace driver_maui.Pages
{
    public class LessonDisplay
    {
        public int LessonId { get; set; }
        public string Date { get; set; } = "";
        public string Time { get; set; } = "";
        public bool Paid { get; set; }
        public int Canceled { get; set; }
        public string StatusText => Paid ? "Paid" : (Canceled == 1 ? "Cancelled" : "Unpaid");
        public Color StatusColor => Paid ? Colors.Green : (Canceled == 1 ? Colors.Gray : Colors.OrangeRed);
    }

    public partial class ViewLessonsPage : ContentPage
    {
        public ViewLessonsPage() => InitializeComponent();

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            ErrorLabel.IsVisible = false;
            try
            {
                var lessons = await ServiceHelper.CallAsync(srv => srv.GetAllStudentLessonsAsync(AppState.UserId));
                LessonsList.ItemsSource = lessons?
                    .Select(l => new LessonDisplay
                    {
                        LessonId = l.LessonId,
                        Date = l.Date,
                        Time = l.Time,
                        Paid = l.paid,
                        Canceled = l.Canceled
                    })
                    .OrderByDescending(l => l.Date)
                    .ToList();
            }
            catch (Exception ex)
            {
                ErrorLabel.Text = $"Error loading lessons: {ex.Message}";
                ErrorLabel.IsVisible = true;
            }
        }

        private async void Back_Click(object s, EventArgs e) => await Shell.Current.GoToAsync("//StudentHome");
    }
}
