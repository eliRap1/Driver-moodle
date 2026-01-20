using driver_client.driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace driver_client
{
    public partial class ViewLessons : Page
    {
        // Display model for the grid
        public class Lesson
        {
            public int LessonId { get; set; }
            public string Date { get; set; }
            public string Time { get; set; }
            public string Paid { get; set; }
            public bool IsPaid { get; set; }
            public DateTime DateTime { get; set; }
        }

        public ViewLessons()
        {
            InitializeComponent();
            LoadLessons();
        }

        private void LoadLessons()
        {
            try
            {
                var client = new Service1Client();
                List<Lessons> allLessons = client.GetAllStudentLessons(LogIn.sign.Id).ToList();
                DateTime now = DateTime.Now;

                var upcomingLessons = new List<Lesson>();
                var completedLessons = new List<Lesson>();

                foreach (var lesson in allLessons)
                {
                    // Skip cancelled lessons
                    if (lesson.Canceled == 1)
                        continue;

                    DateTime lessonDateTime;

                    // Try to parse the date and time
                    if (!DateTime.TryParse($"{lesson.Date} {lesson.Time}", out lessonDateTime))
                    {
                        // If standard parsing fails, try different formats
                        if (!DateTime.TryParseExact($"{lesson.Date} {lesson.Time}",
                            new[] { "dd-MM-yyyy HH:mm", "dd/MM/yyyy HH:mm", "yyyy-MM-dd HH:mm" },
                            System.Globalization.CultureInfo.InvariantCulture,
                            System.Globalization.DateTimeStyles.None,
                            out lessonDateTime))
                        {
                            continue; // Skip this lesson if we can't parse the date
                        }
                    }

                    var item = new Lesson
                    {
                        LessonId = lesson.LessonId,
                        Date = lessonDateTime.ToString("dd-MM-yyyy"),
                        Time = lessonDateTime.ToString("HH:mm"),
                        Paid = lesson.paid ? "Yes" : "No",
                        IsPaid = lesson.paid,
                        DateTime = lessonDateTime
                    };

                    // Upcoming: future lessons
                    if (lessonDateTime >= now)
                        upcomingLessons.Add(item);
                    // Completed: past lessons
                    else
                        completedLessons.Add(item);
                }

                // Sort by date/time
                UpcomingLessonsGrid.ItemsSource = upcomingLessons.OrderBy(l => l.DateTime).ToList();
                CompletedLessonsGrid.ItemsSource = completedLessons.OrderByDescending(l => l.DateTime).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load lessons.\n" + ex.Message,
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MarkPaid_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is Lesson lesson)
            {
                try
                {
                    var result = MessageBox.Show(
                        $"Mark lesson on {lesson.Date} at {lesson.Time} as paid?",
                        "Confirm Payment",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        var client = new Service1Client();
                        client.MarkLessonPaid(lesson.LessonId);
                        MessageBox.Show("Lesson marked as paid!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadLessons(); // Refresh the list
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to mark lesson as paid.\n" + ex.Message,
                                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            page.Navigate(new StudentUI());
        }
    }
}