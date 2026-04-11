using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using driver_client.driver;

namespace driver_client
{
    public partial class StudentCourses : Page
    {
        private int studentId;

        public class CourseViewModel
        {
            public int CourseId { get; set; }
            public string CourseName { get; set; }
            public string Description { get; set; }
            public int TotalModules { get; set; }
            public int CompletedModules { get; set; }
            public int ProgressPercent => TotalModules > 0 ? (CompletedModules * 100 / TotalModules) : 0;
            public string ProgressText => $"{ProgressPercent}%";

            public string ProgressDash
            {
                get
                {
                    double circumference = 3.14159 * 68;
                    double progress = (ProgressPercent / 100.0) * circumference;
                    double remaining = circumference - progress;
                    return $"{progress / 6},{remaining / 6}";
                }
            }
        }

        public StudentCourses()
        {
            InitializeComponent();
            var srv = new Service1Client();
            studentId = srv.GetUserID(LogIn.sign.Username, "Student");
            LoadCourses();
        }

        private void LoadCourses()
        {
            try
            {
                var srv = new Service1Client();
                var courseProgress = srv.GetStudentCourseProgress(studentId);

                if (courseProgress != null && courseProgress.Length > 0)
                {
                    var courses = courseProgress.Select(cp => new CourseViewModel
                    {
                        CourseId = cp.CourseId,
                        CourseName = cp.CourseName ?? "Unknown Course",
                        Description = cp.CourseDescription ?? "",
                        TotalModules = cp.TotalModules,
                        CompletedModules = cp.CompletedModules
                    }).ToList();

                    CoursesItemsControl.ItemsSource = courses;
                    EmptyStatePanel.Visibility = Visibility.Collapsed;
                    return;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading from service: {ex.Message}");
            }

            // No courses available - show empty state
            EmptyStatePanel.Visibility = Visibility.Visible;
        }

        private void Course_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is CourseViewModel course)
            {
                page.Navigate(new CourseDetails(course.CourseId, course.CourseName));
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            page.Navigate(new StudentUI());
        }
    }
}
