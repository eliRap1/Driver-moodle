using driver_client.driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace driver_client
{
    public partial class ScheduleLesson : Page
    {
        private List<UnavailableDay> hardUnavailable;       // TeacherUnavailableDate
        private List<SpecialDay> specialDays;               // TeacherSpacialDays
        private List<string> weeklyAvailableDays;           // Availability.availableDays
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
            var srv = new Service1Client();

            teacherId = srv.GetTeacherId(LogIn.sign.Id);
            Calendars cal = srv.GetTeacherCalendar(teacherId);
            List<Calendars> unavailable = srv.GetTeacherUnavailableDates(teacherId).ToList();
            List<Calendars> special = srv.TeacherSpacialDays(teacherId).ToList();

            studentLessons = srv.GetAllStudentLessons(LogIn.sign.Id).ToList();

            // DEFAULT HOURS
            if (cal != null)
            {
                weeklyAvailableDays = cal.AvailableDays != null
                    ? cal.AvailableDays.ToList()
                    : new List<string>();

                defaultStart = TimeSpan.Parse(string.IsNullOrEmpty(cal.StartTime) ? "08:00" : cal.StartTime);
                defaultEnd = TimeSpan.Parse(string.IsNullOrEmpty(cal.EndTime) ? "20:00" : cal.EndTime);
            }
            else
            {
                weeklyAvailableDays = new List<string>();
                defaultStart = TimeSpan.FromHours(8);
                defaultEnd = TimeSpan.FromHours(20);
            }

            // HARD UNAVAILABLE DAYS (AllDay = true only)
            hardUnavailable = new List<UnavailableDay>();
            foreach (var u in unavailable)
            {
                if (u.UnavailableDays != null)
                    hardUnavailable.AddRange(u.UnavailableDays);
            }

            // SPECIAL DAYS
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
        }

        private void BlackoutUnavailableDates()
        {
            // Only blackout days that are FULLY unavailable (AllDay = true)
            foreach (var u in hardUnavailable.Where(x => x.AllDay))
            {
                lessonDatePicker.BlackoutDates.Add(
                    new CalendarDateRange(u.Date.Date)
                );
            }
        }

        private void LessonDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lessonDatePicker.SelectedDate == null)
                return;

            DateTime selected = lessonDatePicker.SelectedDate.Value;
            string dayName = selected.DayOfWeek.ToString();

            // Check if there's a special day for this date
            SpecialDay special = specialDays.FirstOrDefault(s => s.Date.Date == selected.Date);

            // If there's a special day, the teacher CAN work regardless of weeklyAvailableDays
            // If no special day, check if it's in regular working days
            if (special == null && !weeklyAvailableDays.Contains(dayName))
            {
                MessageBox.Show($"{dayName} is not a teaching day and no special availability set.");
                lessonDatePicker.SelectedDate = null;
                return;
            }

            LoadTimeOptions(selected);
        }

        private void LoadTimeOptions(DateTime day)
        {
            lessonTimeComboBox.Items.Clear();

            // CHECK if day is FULLY unavailable (AllDay = true)
            var fullDayUnavailable = hardUnavailable.FirstOrDefault(u =>
                u.Date.Date == day.Date && u.AllDay);

            if (fullDayUnavailable != null)
            {
                MessageBox.Show("This date is fully unavailable.");
                return;
            }

            // Get partially unavailable times for this day (AllDay = false)
            var partialUnavailable = hardUnavailable
                .Where(u => u.Date.Date == day.Date && !u.AllDay)
                .ToList();

            // SPECIAL DAY? Use special hours, otherwise use default
            SpecialDay special = specialDays.FirstOrDefault(s => s.Date.Date == day.Date);

            TimeSpan start = special != null ? TimeSpan.Parse(special.StartTime) : defaultStart;
            TimeSpan end = special != null ? TimeSpan.Parse(special.EndTime) : defaultEnd;

            // Generate available time slots
            while (start <= end)
            {
                string timeStr = start.ToString(@"hh\:mm");

                // Check if this hour is blocked by partial unavailability
                bool isBlocked = IsTimeBlocked(start, partialUnavailable);

                // Check if student already has lesson at this time
                bool hasLesson = StudentHasLesson(day, timeStr);

                // Only add if not blocked and student doesn't have lesson
                if (!isBlocked && !hasLesson)
                {
                    lessonTimeComboBox.Items.Add(timeStr);
                }

                start = start.Add(TimeSpan.FromHours(1));
            }

            if (lessonTimeComboBox.Items.Count == 0)
                MessageBox.Show("No available hours on this date.");
        }

        /// <summary>
        /// Check if a specific time falls within any unavailable time ranges
        /// </summary>
        private bool IsTimeBlocked(TimeSpan time, List<UnavailableDay> partialUnavailable)
        {
            foreach (var unavail in partialUnavailable)
            {
                // If StartTime or EndTime is null/empty, skip this check
                if (string.IsNullOrEmpty(unavail.StartTime) || string.IsNullOrEmpty(unavail.EndTime))
                    continue;

                TimeSpan unavailStart = TimeSpan.Parse(unavail.StartTime);
                TimeSpan unavailEnd = TimeSpan.Parse(unavail.EndTime);

                // Check if the time falls within the unavailable range
                if (time >= unavailStart && time < unavailEnd)
                {
                    return true; // This time is blocked
                }
            }

            return false; // Time is available
        }

        private bool StudentHasLesson(DateTime date, string time)
        {
            return studentLessons.Any(l =>
                DateTime.Parse(l.Date).Date == date.Date &&
                l.Time == time
            );
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

            var srv = new Service1Client();
            srv.AddLessonForStudent(LogIn.sign.Id, date, time);

            MessageBox.Show($"Lesson booked: {date} at {time}");
            page.Navigate(new StudentUI());
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            page.Navigate(new StudentUI());
        }
    }
}