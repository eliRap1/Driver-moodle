using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using driver_client.driver;

namespace driver_client
{
    public partial class CourseDetails : Page
    {
        private int courseId;
        private string courseName;
        private int studentId;
        private List<ModuleViewModel> modules;

        public class ModuleViewModel
        {
            public int ModuleId { get; set; }
            public string ModuleName { get; set; }
            public string Description { get; set; }
            public int ModuleOrder { get; set; }
            public bool IsCompleted { get; set; }

            public string StatusIcon => IsCompleted ? "\u2713" : ModuleOrder.ToString();
            public Brush StatusBackground => IsCompleted
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#27AE60"))
                : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2A3F5F"));
            public Brush BorderColor => IsCompleted
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#27AE60"))
                : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2A3F5F"));
            public Brush TitleColor => IsCompleted
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#27AE60"))
                : new SolidColorBrush(Colors.White);

            public string ButtonText => IsCompleted ? "Completed" : "Mark Complete";
            public Brush ButtonBackground => IsCompleted
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#27AE60"))
                : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00E0FF"));
            public Brush ButtonForeground => IsCompleted
                ? new SolidColorBrush(Colors.White)
                : new SolidColorBrush(Colors.Black);
            public Brush ButtonBorder => IsCompleted
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#229954"))
                : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00C0CC"));
            public bool CanComplete => !IsCompleted;
        }

        public CourseDetails(int courseId, string courseName)
        {
            InitializeComponent();
            this.courseId = courseId;
            this.courseName = courseName;

            var srv = new Service1Client();
            studentId = srv.GetUserID(LogIn.sign.Username, "Student");

            CourseTitle.Text = courseName;
            LoadModules();
        }

        private void LoadModules()
        {
            try
            {
                // Try to load from service first
                var srv = new Service1Client();
                var courseModules = srv.GetCourseModules(courseId);
                var completedModules = srv.GetStudentCompletedModules(studentId);

                if (courseModules != null && courseModules.Length > 0)
                {
                    int order = 1;
                    modules = courseModules.Select(cm => new ModuleViewModel
                    {
                        ModuleId = cm.ModuleId,
                        ModuleName = cm.ModuleName ?? "Unknown Module",
                        Description = cm.Description ?? "",
                        ModuleOrder = order++,
                        IsCompleted = completedModules?.Any(comp => comp.ModuleId == cm.ModuleId && comp.IsCompleted) ?? false
                    }).ToList();

                    ModulesItemsControl.ItemsSource = modules;
                    UpdateProgressDisplay();
                    return;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading from service: {ex.Message}");
            }

            // No modules available from service
            modules = new List<ModuleViewModel>();
            UpdateProgressDisplay();
        }

        private void UpdateProgressDisplay()
        {
            int completed = modules.Count(m => m.IsCompleted);
            int total = modules.Count;
            int percent = total > 0 ? (completed * 100 / total) : 0;

            CompletedCountText.Text = completed.ToString();
            TotalCountText.Text = total.ToString();
            ProgressPercentText.Text = $"{percent}% Complete";

            ProgressBar.Width = (percent / 100.0) * 250;
        }

        private void MarkComplete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int moduleId)
            {
                var module = modules.FirstOrDefault(m => m.ModuleId == moduleId);
                if (module != null && !module.IsCompleted)
                {
                    try
                    {
                        // Try to update in service
                        var srv = new Service1Client();
                        bool success = srv.MarkModuleComplete(studentId, moduleId);

                        if (success)
                        {
                            module.IsCompleted = true;
                            ModulesItemsControl.ItemsSource = null;
                            ModulesItemsControl.ItemsSource = modules;
                            UpdateProgressDisplay();

                            MessageBox.Show($"Module '{module.ModuleName}' marked as complete!",
                                            "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Fallback - just update locally
                        System.Diagnostics.Debug.WriteLine($"Service error: {ex.Message}");
                        module.IsCompleted = true;
                        ModulesItemsControl.ItemsSource = null;
                        ModulesItemsControl.ItemsSource = modules;
                        UpdateProgressDisplay();

                        MessageBox.Show($"Module '{module.ModuleName}' marked as complete!",
                                        "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            page.Navigate(new StudentCourses());
        }
    }
}
