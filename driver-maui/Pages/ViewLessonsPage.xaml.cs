using System.Globalization;
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
        public DateTime Sortable { get; set; }
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
                        Canceled = l.Canceled,
                        Sortable = ParseLessonDateTime(l.Date, l.Time)
                    })
                    .OrderByDescending(l => l.Sortable)
                    .ToList();
            }
            catch (Exception ex)
            {
                ErrorLabel.Text = $"Error loading lessons: {ex.Message}";
                ErrorLabel.IsVisible = true;
            }
        }

        private static DateTime ParseLessonDateTime(string date, string time)
        {
            string[] formats = { "yyyy-MM-dd HH:mm", "dd-MM-yyyy HH:mm", "dd/MM/yyyy HH:mm", "yyyy-MM-dd H:mm", "MM/dd/yyyy HH:mm:ss" };
            string combined = $"{date} {time}";
            if (DateTime.TryParseExact(combined, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                return dt;
            if (DateTime.TryParse(combined, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                return dt;
            DateTime.TryParse(combined, out dt);
            return dt;
        }

        private async void Back_Click(object s, EventArgs e) => await Shell.Current.GoToAsync("//StudentHome");
    }
}
