using driver_maui.Services;
using DriverService;

namespace driver_maui.Pages
{
    public partial class ScheduleLessonPage : ContentPage
    {
        private int _teacherId;

        public ScheduleLessonPage()
        {
            InitializeComponent();
            DatePicker1.MinimumDate = DateTime.Today;
            DatePicker1.Date = DateTime.Today;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (!await AppState.RequireRoleAsync(this, "Student")) return;
            await LoadTeacher();
            await LoadTimes();
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

        private async Task LoadTimes()
        {
            TimePicker1.ItemsSource = null;
            NoTimesLabel.IsVisible = false;
            DateTime selected = DatePicker1.Date;

            try
            {
                Calendars cal = null;
                try { cal = await ServiceHelper.CallAsync(srv => srv.GetTeacherCalendarAsync(_teacherId)); } catch { }

                TimeSpan start = TimeSpan.FromHours(8);
                TimeSpan end = TimeSpan.FromHours(20);
                List<string> availableDays = null;

                if (cal != null)
                {
                    if (!TimeSpan.TryParse(cal.StartTime, out start)) start = TimeSpan.FromHours(8);
                    if (!TimeSpan.TryParse(cal.EndTime, out end)) end = TimeSpan.FromHours(20);
                    if (cal.AvailableDays != null) availableDays = cal.AvailableDays.ToList();
                }

                if (availableDays != null && availableDays.Count > 0 &&
                    !availableDays.Contains(selected.DayOfWeek.ToString()))
                {
                    NoTimesLabel.IsVisible = true;
                    return;
                }

                var taken = new HashSet<string>();
                try
                {
                    var existing = await ServiceHelper.CallAsync(srv => srv.GetAllStudentLessonsAsync(AppState.UserId));
                    string isoDate = selected.ToString("yyyy-MM-dd");
                    string altDate = selected.ToString("dd-MM-yyyy");
                    string slashDate = selected.ToString("dd/MM/yyyy");
                    if (existing != null)
                    {
                        foreach (var l in existing)
                        {
                            if (l.Canceled == 1) continue;
                            if (l.Date == isoDate || l.Date == altDate || l.Date == slashDate)
                                taken.Add(l.Time);
                        }
                    }
                }
                catch { }

                var slots = new List<string>();
                for (var t = start; t < end; t = t.Add(TimeSpan.FromHours(1)))
                {
                    string s = t.ToString(@"hh\:mm");
                    if (!taken.Contains(s)) slots.Add(s);
                }

                if (slots.Count == 0)
                {
                    NoTimesLabel.IsVisible = true;
                }
                else
                {
                    TimePicker1.ItemsSource = slots;
                    TimePicker1.SelectedIndex = 0;
                }
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"LoadTimes: {ex}"); }
        }

        private async void DatePicker1_DateSelected(object sender, DateChangedEventArgs e)
        {
            await LoadTimes();
        }

        private async void Book_Click(object sender, EventArgs e)
        {
            ResultLabel.IsVisible = false;
            string time = TimePicker1.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(time))
            {
                ShowResult("Pick a time first.", Colors.Red);
                return;
            }

            string date = DatePicker1.Date.ToString("yyyy-MM-dd");

            try
            {
                await ServiceHelper.CallAsync(srv => srv.AddLessonForStudentAsync(AppState.UserId, date, time));
                ShowResult($"Booked {DatePicker1.Date:dd/MM/yyyy} at {time}.", Colors.Green);
                await LoadTimes();
            }
            catch (Exception ex)
            {
                ShowResult($"Booking failed: {ex.Message}", Colors.Red);
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
