using driver_client.driver;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace driver_client
{
    public partial class ScheduleLesson : Page
    {
        private List<UnavailableDay> hardUnavailable;
        private List<SpecialDay> specialDays;
        private List<string> weeklyAvailableDays;
        private TimeSpan defaultStart;
        private TimeSpan defaultEnd;

        private int teacherId;
        private List<Lessons> studentLessons;

        public ScheduleLesson()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            teacherId = ClientSession.TeacherId;
            Calendars cal = ServiceGateway.Use(client => client.GetTeacherCalendar(teacherId));
            List<Calendars> unavailable = (ServiceGateway.Use(client => client.GetTeacherUnavailableDates(teacherId)) ?? new Calendars[0]).ToList();
            List<Calendars> special = (ServiceGateway.Use(client => client.TeacherSpacialDays(teacherId)) ?? new Calendars[0]).ToList();

            studentLessons = (ServiceGateway.Use(client => client.GetAllStudentLessons(ClientSession.StudentId)) ?? new Lessons[0]).ToList();

            if (cal != null)
            {
                weeklyAvailableDays = cal.AvailableDays != null
                    ? cal.AvailableDays.ToList()
                    : new List<string>();

                defaultStart = ParseTimeOrDefault(cal.StartTime, TimeSpan.FromHours(8));
                defaultEnd = ParseTimeOrDefault(cal.EndTime, TimeSpan.FromHours(20));
            }
            else
            {
                weeklyAvailableDays = new List<string>();
                defaultStart = TimeSpan.FromHours(8);
                defaultEnd = TimeSpan.FromHours(20);
            }

            hardUnavailable = new List<UnavailableDay>();
            foreach (var u in unavailable)
            {
                if (u.UnavailableDays != null)
                    hardUnavailable.AddRange(u.UnavailableDays);
            }

            specialDays = new List<SpecialDay>();
            foreach (var s in special)
            {
                if (s.SpecialDaysList != null)
                    specialDays.AddRange(s.SpecialDaysList);
            }

            lessonDatePicker.DisplayDateStart = DateTime.Today;
            lessonDatePicker.BlackoutDates.Clear();
            BlackoutUnavailableDates();

            lessonDatePicker.SelectedDateChanged += LessonDateChanged;
            UpdateBookingSummary();
        }

        private TimeSpan ParseTimeOrDefault(string value, TimeSpan fallback)
        {
            return TimeSpan.TryParse(value, out TimeSpan parsed) ? parsed : fallback;
        }

        private void BlackoutUnavailableDates()
        {
            foreach (var u in hardUnavailable.Where(x => x.AllDay))
            {
                lessonDatePicker.BlackoutDates.Add(new CalendarDateRange(u.Date.Date));
            }
        }

        private void LessonDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lessonDatePicker.SelectedDate == null)
            {
                lessonTimeComboBox.Items.Clear();
                AvailabilityText.Text = "Select a date to see open lesson times.";
                UpdateBookingSummary();
                return;
            }

            DateTime selected = lessonDatePicker.SelectedDate.Value;
            string dayName = selected.DayOfWeek.ToString();

            SpecialDay special = specialDays.FirstOrDefault(s => s.Date.Date == selected.Date);
            if (special == null && !weeklyAvailableDays.Contains(dayName))
            {
                MessageBox.Show($"{dayName} is not a teaching day and no special availability set.");
                lessonDatePicker.SelectedDate = null;
                UpdateBookingSummary();
                return;
            }

            LoadTimeOptions(selected);
            UpdateBookingSummary();
        }

        private void LoadTimeOptions(DateTime day)
        {
            lessonTimeComboBox.Items.Clear();
            AvailabilityText.Text = "Checking availability...";

            var fullDayUnavailable = hardUnavailable.FirstOrDefault(u =>
                u.Date.Date == day.Date && u.AllDay);

            if (fullDayUnavailable != null)
            {
                AvailabilityText.Text = "This date is fully unavailable.";
                return;
            }

            var partialUnavailable = hardUnavailable
                .Where(u => u.Date.Date == day.Date && !u.AllDay)
                .ToList();

            SpecialDay special = specialDays.FirstOrDefault(s => s.Date.Date == day.Date);

            TimeSpan start = special != null ? ParseTimeOrDefault(special.StartTime, defaultStart) : defaultStart;
            TimeSpan end = special != null ? ParseTimeOrDefault(special.EndTime, defaultEnd) : defaultEnd;

            while (start < end)
            {
                string timeStr = start.ToString(@"hh\:mm");
                if (!IsTimeBlocked(start, partialUnavailable) && !StudentHasLesson(day, timeStr))
                    lessonTimeComboBox.Items.Add(timeStr);

                start = start.Add(TimeSpan.FromHours(1));
            }

            if (lessonTimeComboBox.Items.Count == 0)
            {
                AvailabilityText.Text = "No available hours on this date.";
            }
            else
            {
                AvailabilityText.Text = $"{lessonTimeComboBox.Items.Count} available time slots for {day:dd/MM/yyyy}.";
                lessonTimeComboBox.SelectedIndex = 0;
            }
        }

        private bool IsTimeBlocked(TimeSpan time, List<UnavailableDay> partialUnavailable)
        {
            foreach (var unavail in partialUnavailable)
            {
                if (string.IsNullOrEmpty(unavail.StartTime) || string.IsNullOrEmpty(unavail.EndTime))
                    continue;

                TimeSpan unavailStart = ParseTimeOrDefault(unavail.StartTime, TimeSpan.Zero);
                TimeSpan unavailEnd = ParseTimeOrDefault(unavail.EndTime, TimeSpan.Zero);

                if (time >= unavailStart && time < unavailEnd)
                    return true;
            }

            return false;
        }

        private bool StudentHasLesson(DateTime date, string time)
        {
            return studentLessons.Any(l =>
                TryParseLessonDate(l.Date, out DateTime lessonDate) &&
                lessonDate.Date == date.Date &&
                l.Time == time);
        }

        private bool TryParseLessonDate(string value, out DateTime result)
        {
            string[] formats = { "yyyy-MM-dd", "dd-MM-yyyy", "dd/MM/yyyy" };
            return DateTime.TryParseExact(value, formats, CultureInfo.InvariantCulture,
                       DateTimeStyles.None, out result) ||
                   DateTime.TryParse(value, out result);
        }

        private void LessonTimeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateBookingSummary();
        }

        private void UpdateBookingSummary()
        {
            SelectedDateText.Text = lessonDatePicker.SelectedDate.HasValue
                ? lessonDatePicker.SelectedDate.Value.ToString("dddd, dd/MM/yyyy")
                : "Not selected";

            SelectedTimeText.Text = lessonTimeComboBox.SelectedItem != null
                ? lessonTimeComboBox.SelectedItem.ToString()
                : "Not selected";
        }

        private void ConfirmLesson_Click(object sender, RoutedEventArgs e)
        {
            if (lessonDatePicker.SelectedDate == null || lessonTimeComboBox.SelectedItem == null)
            {
                MessageBox.Show("Select date and time first.");
                return;
            }

            string date = lessonDatePicker.SelectedDate.Value.ToString("yyyy-MM-dd");
            string time = lessonTimeComboBox.SelectedItem.ToString();

            try
            {
                ServiceGateway.Use(client => client.AddLessonForStudent(ClientSession.StudentId, date, time));
                MessageBox.Show($"Lesson booked: {date} at {time}");
                page.Navigate(new StudentUI());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Booking failed: " + ex.Message, "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            page.Navigate(new StudentUI());
        }
    }
}
