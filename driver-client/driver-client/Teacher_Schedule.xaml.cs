using driver_client.driver;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace driver_client
{
    public partial class Teacher_Schedule : Page
    {
        // Wrapper class for UI display
        public class LessonDisplay
        {
            public int LessonId { get; set; }
            public int StudentId { get; set; }
            public string StudentName { get; set; }
            public string Date { get; set; }
            public string Time { get; set; }
            public string PaidStatus { get; set; }
            public DateTime RawDateTime { get; set; }

            // Dynamic fields for UI
            public string StatusText { get; set; }
            public string StatusColor { get; set; } // Use string for XAML binding
            public Visibility CanCancel { get; set; }
            public string PaidButtonText { get; set; }
        }

        private DispatcherTimer RefreshTimer;

        public Teacher_Schedule()
        {
            InitializeComponent();
            LessonDatePicker.SelectedDate = DateTime.Now;

            // Subscribe to the DatePicker change event to auto-update the list
            LessonDatePicker.SelectedDateChanged += LessonDatePicker_SelectedDateChanged;

            LoadAll();

            RefreshTimer = new DispatcherTimer();
            RefreshTimer.Interval = TimeSpan.FromSeconds(3);
            RefreshTimer.Tick += RefreshTimer_Tick; // Use a dedicated tick handler
            RefreshTimer.Start();
        }

        // Dedicated tick handler for the timer
        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            LoadAll();
        }

        // Handle DatePicker change to update the "Scheduled Lessons" tab automatically
        private void LessonDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            // Call LoadAll which will automatically refresh the filtered list based on the new date
            LoadAll();
        }

        // Helper to process lesson data for display properties
        private LessonDisplay ProcessLessonDisplay(Lessons l, DateTime dt, Dictionary<int, string> studentLookup)
        {
            bool isCanceled = l.Canceled == 1;
            bool isUpcoming = dt > DateTime.Now;

            // Try to get the name, default to "Unknown" if missing
            string name = "Unknown";
            if (studentLookup.ContainsKey(l.StudentId))
            {
                name = studentLookup[l.StudentId];
            }

            return new LessonDisplay
            {
                LessonId = l.LessonId,
                StudentId = l.StudentId,
                StudentName = name, // <--- Assign the name here
                Date = dt.ToString("dd/MM/yyyy"),
                Time = dt.ToString("HH:mm"),
                PaidStatus = l.paid ? "Yes" : "No",
                RawDateTime = dt,

                StatusText = isCanceled ? "Canceled" : "Active",
                StatusColor = isCanceled ? "Red" : "Lime",

                PaidButtonText = l.paid ? "Paid" : "Mark Paid",

                CanCancel = (isUpcoming && !isCanceled)
                    ? Visibility.Visible
                    : Visibility.Collapsed
            };
        }

        // Convert raw lessons to display format
        private List<LessonDisplay> ConvertToDisplay(List<Lessons> raw, Dictionary<int, string> studentLookup)
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

                // Pass the lookup dictionary here
                list.Add(ProcessLessonDisplay(l, dt, studentLookup));
            }

            return list;
        }

        // ------------ MASTER LOAD (Modified to not use sender/event args) ------------
        private void LoadAll()
        {
            try
            {
                var srv = new Service1Client();
                List<Lessons> raw = srv.GetAllTeacherLessons(LogIn.sign.Id).ToList();

                // --- NEW: Create a lookup dictionary for Student Names ---
                // 1. Get unique student IDs from the list
                var uniqueStudentIds = raw.Select(x => x.StudentId).Distinct();

                // 2. Fetch details for each student
                AllUsers allStudents = srv.GetAllUsers();
                Dictionary<int, string> studentLookup = new Dictionary<int, string>();
                foreach (var id in uniqueStudentIds)
                {
                    var student = allStudents.FirstOrDefault(x => x.StudentId == id);
                    if (student != null)
                    {
                        studentLookup[id] = student.Username;
                    }
                }
                // ---------------------------------------------------------

                // Pass the lookup dictionary to the conversion method
                List<LessonDisplay> allDisplayLessons = ConvertToDisplay(raw, studentLookup);

                // HISTORY
                HistoryLessons.ItemsSource = allDisplayLessons
                    .OrderByDescending(x => x.RawDateTime)
                    .ToList();

                // UPCOMING
                UpcomingLessons.ItemsSource = allDisplayLessons
                    .Where(x => x.RawDateTime > DateTime.Now)
                    .OrderBy(x => x.RawDateTime)
                    .ToList();

                // SCHEDULED
                if (LessonDatePicker.SelectedDate.HasValue)
                {
                    ShowLessonsForDate(LessonDatePicker.SelectedDate.Value, allDisplayLessons);
                }
                else
                {
                    ShowLessonsForDate(DateTime.Now, allDisplayLessons);
                }
            }
            catch (Exception ex)
            {
                if (RefreshTimer == null || !RefreshTimer.IsEnabled)
                {
                    MessageBox.Show("Failed to load lessons.\n" + ex.Message);
                }
            }
        }

        // Overload to support the old timer/initial call signatures (calls the new LoadAll())
        private void LoadAll(object sender, EventArgs e)
        {
            LoadAll();
        }

        // ------------ SHOW LESSONS FOR SELECTED DATE (REMOVED redundant click handler) ------------
        // The ShowLessons_Click method is now obsolete and should be removed from the XAML and C#.

        private void ShowLessonsForDate(DateTime day, List<LessonDisplay> all)
        {
            // Only show lessons whose date component matches the selected day
            var filtered = all
                .Where(x => x.RawDateTime.Date == day.Date)
                .OrderBy(x => x.RawDateTime) // Order by time of day
                .ToList();

            DayLessons.ItemsSource = filtered;
        }

        // ------------ CANCEL A LESSON ------------
        private void CancelLesson_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            var lesson = btn.CommandParameter as LessonDisplay; // Get the lesson from CommandParameter

            if (lesson == null) return;

            if (MessageBox.Show(
                $"Are you sure you want to cancel lesson ID {lesson.LessonId} on {lesson.Date} at {lesson.Time}?",
                "Confirm Cancel",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning) == MessageBoxResult.No)
                return;

            try
            {
                var srv = new Service1Client();

                // Assuming this server method marks the lesson as Canceled = 1
                srv.CancelLesson(lesson.LessonId);

                MessageBox.Show("Lesson successfully canceled.");

                // Reload all data to refresh all ListViews (Upcoming, Scheduled, History)
                LoadAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to cancel lesson.\n" + ex.Message);
            }
        }

        // ------------ TOGGLE PAID STATUS (Mark Paid) ------------
        private void TogglePaid_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            var lessonToUpdate = btn.CommandParameter as LessonDisplay;

            if (lessonToUpdate == null)
            {
                MessageBox.Show("Could not retrieve lesson information.");
                return;
            }

            // Do not execute if already paid
            if (lessonToUpdate.PaidButtonText == "Paid")
            {
                MessageBox.Show("This lesson is already marked as paid.");
                return;
            }

            try
            {
                var srv = new Service1Client();

                // 1. Mark the lesson itself as paid in the Lessons table (must be done before creating Payment record)
                // Note: The order you had them in was a bit mixed. I'll put MarkLessonPaid first, 
                // assuming it's required for the Payment object logic to work cleanly.

                // 2. Create the Payment record
                Payment pay = new Payment
                {
                    StudentID = lessonToUpdate.StudentId,
                    PaymentID = lessonToUpdate.LessonId,
                    TeacherID = LogIn.sign.Id,
                    PaymentMethod = "Teacher",
                    PaymentDate = DateTime.Now,
                    NumberOfPayments = 1,
                    paid = true,
                    // Get LessonPrice from the logged-in teacher
                    Amount = srv.GetUserById(LogIn.sign.Id, "Teacher").LessonPrice
                };

                srv.Pay(pay);

                MessageBox.Show($"Lesson ID {lessonToUpdate.LessonId} successfully marked as paid.");

                // Reload all lists to reflect the status change on all tabs
                LoadAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to update paid status.\nEnsure all required data is available (e.g., Lesson Price).\n" + ex.Message);
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            // Stop the timer before navigating away
            RefreshTimer.Stop();
            page.Navigate(new TeacherUI());
        }
    }
}