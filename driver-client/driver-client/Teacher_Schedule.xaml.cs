using driver_client.driver;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace driver_client
{
    public partial class Teacher_Schedule : Page
    {
        // Wrapper class for UI display
        public class LessonDisplay
        {
            public int LessonId { get; set; }
            public int StudentId { get; set; }
            public string Date { get; set; }
            public string Time { get; set; }
            public string PaidStatus { get; set; }
            public DateTime RawDateTime { get; set; }

            public string StatusText { get; set; }
            public string StatusColor { get; set; }
            public Visibility CanCancel { get; set; }
            public string PaidButtonText { get; set; }
        }

        public Teacher_Schedule()
        {
            InitializeComponent();
            LessonDatePicker.SelectedDate = DateTime.Now;

            LoadAll();
        }

        // ------------ MASTER LOAD ------------
        private void LoadAll()
        {
            try
            {
                var srv = new Service1Client();
                List<Lessons> raw = srv.GetAllTeacherLessons(LogIn.sign.Id).ToList();

                List<LessonDisplay> all = new List<LessonDisplay>();

                foreach (var l in raw)
                {
                    if (!DateTime.TryParseExact($"{l.Date} {l.Time}",
                        "dd-MM-yyyy HH:mm",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out DateTime dt))
                    {
                        // try fallback (your history format)
                        if (!DateTime.TryParse($"{l.Date} {l.Time}", out dt))
                            continue;
                    }

                    all.Add(new LessonDisplay
                    {
                        LessonId = l.LessonId,
                        StudentId = l.StudentId,
                        Date = dt.ToString("dd/MM/yyyy"),
                        Time = dt.ToString("HH:mm"),
                        PaidStatus = l.paid ? "Yes" : "No",
                        RawDateTime = dt
                    });
                }

                // HISTORY (all lessons)
                HistoryLessons.ItemsSource = all.OrderByDescending(x => x.RawDateTime).ToList();

                // UPCOMING = future only
                UpcomingLessons.ItemsSource = all
                    .Where(x => x.RawDateTime > DateTime.Now)
                    .OrderBy(x => x.RawDateTime)
                    .ToList();

                // SELECTED DATE tab is filled only when button clicked.
                // But we can pre-load today's lessons:
                ShowLessonsForDate(DateTime.Now, all);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load lessons.\n" + ex.Message);
            }
        }

        // ------------ SHOW LESSONS FOR SELECTED DATE ------------
        private void ShowLessons_Click(object sender, RoutedEventArgs e)
        {
            if (LessonDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Please select a date.");
                return;
            }

            var srv = new Service1Client();
            List<Lessons> raw = srv.GetAllTeacherLessons(LogIn.sign.Id).ToList();

            List<LessonDisplay> all = ConvertToDisplay(raw);

            ShowLessonsForDate(LessonDatePicker.SelectedDate.Value, all);
        }

        private void ShowLessonsForDate(DateTime day, List<LessonDisplay> all)
        {
            var filtered = all.Where(x => x.RawDateTime.Date == day.Date).ToList();
            DayLessons.ItemsSource = filtered;
        }

        // Convert raw lessons to display format
        private List<LessonDisplay> ConvertToDisplay(List<Lessons> raw)
        {
            List<LessonDisplay> list = new List<LessonDisplay>();

            foreach (var l in raw)
            {
                if (!DateTime.TryParseExact($"{l.Date} {l.Time}",
                        "dd-MM-yyyy HH:mm",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out DateTime dt))
                {
                    if (!DateTime.TryParse($"{l.Date} {l.Time}", out dt))
                        continue;
                }

                list.Add(new LessonDisplay
                {
                    LessonId = l.LessonId,
                    StudentId = l.StudentId,
                    Date = dt.ToString("dd/MM/yyyy"),
                    Time = dt.ToString("HH:mm"),
                    PaidStatus = l.paid ? "Yes" : "No",
                    RawDateTime = dt,

                    // NEW FIELDS
                    StatusText = l.Canceled == 1 ? "Canceled" : "Active",
                    StatusColor = l.Canceled == 1 ? "Red" : "Lime",

                    PaidButtonText = l.paid ? "Paid" : "Mark Paid",

                    // Cancel only if future AND not canceled
                    CanCancel = (dt > DateTime.Now && l.Canceled == 0)
                ? Visibility.Visible
                : Visibility.Collapsed
                });

            }

            return list;
        }

        // ------------ CANCEL A LESSON ------------
        private void CancelLesson_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            var lesson = btn.CommandParameter as LessonDisplay;

            if (lesson == null)
                return;

            if (MessageBox.Show(
                $"Cancel lesson ID {lesson.LessonId}?",
                "Confirm Cancel",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning) == MessageBoxResult.No)
                return;

            try
            {
                var srv = new Service1Client();
                srv.CancelLesson(lesson.LessonId);

                MessageBox.Show("Lesson canceled.");

                LoadAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to cancel lesson.\n" + ex.Message);
            }
        }


        private void Back_Click(object sender, RoutedEventArgs e)
        {
            page.Navigate(new TeacherUI());
        }
        private void TogglePaid_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var srv = new Service1Client();
                Payment pay = new Payment();
                pay.StudentID = ((LessonDisplay)DayLessons.SelectedItem).StudentId;
                pay.PaymentID = ((LessonDisplay)DayLessons.SelectedItem).LessonId;
                pay.TeacherID = LogIn.sign.Id;
                pay.PaymentMethod = "Teacher";
                pay.PaymentDate = DateTime.Now;
                pay.NumberOfPayments = 1;
                pay.paid = true;
                pay.Amount = int.Parse(srv.GetUserById(LogIn.sign.Id, "Teacher").LessonPrice);
                srv.Pay(pay);
                    srv.MarkLessonPaid(((LessonDisplay)DayLessons.SelectedItem).LessonId);
            }
            catch (Exception ex) {
                MessageBox.Show("Click on the lesson then mark as paid.\n" + ex.Message);
            }
        }
    }
}
