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
                List<Lessons> allLessons = (ServiceGateway.Use(client => client.GetAllStudentLessons(ClientSession.StudentId)) ?? new Lessons[0]).ToList();
                DateTime now = DateTime.Now;

                var upcomingLessons = new List<Lesson>();
                var completedLessons = new List<Lesson>();

                foreach (var lesson in allLessons)
                {
                    // Skip cancelled lessons
                    if (lesson.Canceled == 1)
                        continue;

                    DateTime lessonDateTime;
                    string combined = $"{lesson.Date} {lesson.Time}";
                    string[] formats = {
                        "yyyy-MM-dd HH:mm", "yyyy-MM-dd H:mm",
                        "dd-MM-yyyy HH:mm", "dd-MM-yyyy H:mm",
                        "dd/MM/yyyy HH:mm", "dd/MM/yyyy H:mm",
                        "MM/dd/yyyy HH:mm:ss", "M/d/yyyy h:mm:ss tt"
                    };

                    if (!DateTime.TryParseExact(combined, formats,
                            System.Globalization.CultureInfo.InvariantCulture,
                            System.Globalization.DateTimeStyles.None,
                            out lessonDateTime) &&
                        !DateTime.TryParse(combined,
                            System.Globalization.CultureInfo.InvariantCulture,
                            System.Globalization.DateTimeStyles.None,
                            out lessonDateTime) &&
                        !DateTime.TryParse(combined, out lessonDateTime))
                    {
                        System.Diagnostics.Debug.WriteLine($"ViewLessons: cannot parse '{combined}'");
                        continue;
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

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            page.Navigate(new StudentUI());
        }
    }
}
