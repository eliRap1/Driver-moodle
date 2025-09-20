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
                    if (DateTime.TryParse($"{lesson.Date} {lesson.Time}", out DateTime lessonDateTime))
                    {
                        var item = new Lesson
                        {
                            LessonId = lesson.LessonId,
                            Date = lessonDateTime.ToString("dd-MM-yyyy"),
                            Time = lessonDateTime.ToString("HH:mm"),
                            Paid = lesson.paid ? "Yes" : "No"
                        };

                        if (lessonDateTime >= now)
                            upcomingLessons.Add(item);
                        else
                            completedLessons.Add(item);
                    }
                }

                UpcomingLessonsGrid.ItemsSource = upcomingLessons;
                CompletedLessonsGrid.ItemsSource = completedLessons;
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
                    var client = new Service1Client();
                    client.MarkLessonPaid(lesson.LessonId);  // implement on server side
                    LoadLessons();                            // refresh grids
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
