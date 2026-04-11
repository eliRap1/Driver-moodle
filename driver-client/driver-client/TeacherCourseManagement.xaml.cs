using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace driver_client
{
    public partial class TeacherCourseManagement : Page
    {
        private int? editingCourseId = null;
        private int? editingModuleId = null;
        private int currentModuleCourseId = 0;

        public TeacherCourseManagement()
        {
            InitializeComponent();
            LoadCourses();
        }

        private void LoadCourses()
        {
            try
            {
                var srv = new driver.Service1Client();
                var courses = srv.GetAllCourses();

                if (courses != null && courses.Length > 0)
                {
                    var viewModels = new List<CourseListViewModel>();

                    foreach (var c in courses)
                    {
                        var modules = srv.GetCourseModules(c.Id);
                        var vm = new CourseListViewModel
                        {
                            CourseId = c.Id,
                            CourseName = c.CourseName ?? "",
                            Description = c.Description ?? "",
                            DisplayOrder = c.DisplayOrder,
                            IsActive = c.IsActive,
                            Modules = new List<ModuleListViewModel>()
                        };

                        if (modules != null)
                        {
                            foreach (var m in modules)
                            {
                                vm.Modules.Add(new ModuleListViewModel
                                {
                                    ModuleId = m.ModuleId,
                                    ModuleName = m.ModuleName ?? "",
                                    Description = m.Description ?? "",
                                    ContentType = m.ContentType ?? "Text",
                                    DurationMinutes = m.DurationMinutes,
                                    OrderIndex = m.OrderIndex,
                                    IsRequired = m.IsRequired
                                });
                            }
                        }

                        viewModels.Add(vm);
                    }

                    CoursesList.ItemsSource = viewModels;
                    EmptyState.Visibility = Visibility.Collapsed;
                }
                else
                {
                    CoursesList.ItemsSource = null;
                    EmptyState.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading courses: " + ex.Message, "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NewCourse_Click(object sender, RoutedEventArgs e)
        {
            editingCourseId = null;
            FormTitle.Text = "New Course";
            CourseNameBox.Text = "";
            CourseDescBox.Text = "";
            DisplayOrderBox.Text = "1";
            IsActiveCheck.IsChecked = true;
            CourseFormPanel.Visibility = Visibility.Visible;
        }

        private void EditCourse_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag == null) return;

            int courseId = Convert.ToInt32(button.Tag);

            try
            {
                var srv = new driver.Service1Client();
                var course = srv.GetCourseById(courseId);
                if (course != null)
                {
                    editingCourseId = courseId;
                    FormTitle.Text = "Edit Course";
                    CourseNameBox.Text = course.CourseName ?? "";
                    CourseDescBox.Text = course.Description ?? "";
                    DisplayOrderBox.Text = course.DisplayOrder.ToString();
                    IsActiveCheck.IsChecked = course.IsActive;
                    CourseFormPanel.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading course: " + ex.Message, "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveCourse_Click(object sender, RoutedEventArgs e)
        {
            string name = CourseNameBox.Text?.Trim();
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Please enter a course name.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int displayOrder;
            if (!int.TryParse(DisplayOrderBox.Text, out displayOrder))
                displayOrder = 1;

            try
            {
                var srv = new driver.Service1Client();

                var course = new driver.Course
                {
                    CourseName = name,
                    Description = CourseDescBox.Text?.Trim() ?? "",
                    DisplayOrder = displayOrder,
                    IsActive = IsActiveCheck.IsChecked == true
                };

                if (editingCourseId.HasValue)
                {
                    course.Id = editingCourseId.Value;
                    srv.UpdateCourse(course);
                    MessageBox.Show("Course updated!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    srv.AddCourse(course);
                    MessageBox.Show("Course created!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }

                CourseFormPanel.Visibility = Visibility.Collapsed;
                editingCourseId = null;
                LoadCourses();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving course: " + ex.Message, "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelForm_Click(object sender, RoutedEventArgs e)
        {
            CourseFormPanel.Visibility = Visibility.Collapsed;
            editingCourseId = null;
        }

        private void DeactivateCourse_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag == null) return;

            int courseId = Convert.ToInt32(button.Tag);

            var result = MessageBox.Show("Deactivate this course? Students will no longer see it.",
                "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var srv = new driver.Service1Client();
                    srv.DeactivateCourse(courseId);
                    LoadCourses();
                    MessageBox.Show("Course deactivated.", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deactivating course: " + ex.Message, "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void AddModule_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag == null) return;

            currentModuleCourseId = Convert.ToInt32(button.Tag);
            editingModuleId = null;
            ModuleFormTitle.Text = "Add Module";
            ModuleNameBox.Text = "";
            ModuleDescBox.Text = "";
            ContentTypeCombo.SelectedIndex = 0;
            DurationBox.Text = "30";
            OrderIndexBox.Text = "1";
            IsRequiredCheck.IsChecked = true;
            ModuleFormOverlay.Visibility = Visibility.Visible;
        }

        private void EditModule_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag == null) return;

            int moduleId = Convert.ToInt32(button.Tag);

            // Find the module in current data
            var allCourses = CoursesList.ItemsSource as List<CourseListViewModel>;
            if (allCourses == null) return;

            foreach (var course in allCourses)
            {
                var module = course.Modules.FirstOrDefault(m => m.ModuleId == moduleId);
                if (module != null)
                {
                    editingModuleId = moduleId;
                    currentModuleCourseId = course.CourseId;
                    ModuleFormTitle.Text = "Edit Module";
                    ModuleNameBox.Text = module.ModuleName;
                    ModuleDescBox.Text = module.Description;
                    DurationBox.Text = module.DurationMinutes.ToString();
                    OrderIndexBox.Text = module.OrderIndex.ToString();
                    IsRequiredCheck.IsChecked = module.IsRequired;

                    // Set content type combo
                    for (int i = 0; i < ContentTypeCombo.Items.Count; i++)
                    {
                        var item = ContentTypeCombo.Items[i] as ComboBoxItem;
                        if (item != null && item.Content.ToString() == module.ContentType)
                        {
                            ContentTypeCombo.SelectedIndex = i;
                            break;
                        }
                    }

                    ModuleFormOverlay.Visibility = Visibility.Visible;
                    return;
                }
            }
        }

        private void SaveModule_Click(object sender, RoutedEventArgs e)
        {
            string name = ModuleNameBox.Text?.Trim();
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Please enter a module name.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int duration;
            if (!int.TryParse(DurationBox.Text, out duration))
                duration = 30;

            int orderIndex;
            if (!int.TryParse(OrderIndexBox.Text, out orderIndex))
                orderIndex = 1;

            string contentType = "Text";
            var selectedItem = ContentTypeCombo.SelectedItem as ComboBoxItem;
            if (selectedItem != null)
                contentType = selectedItem.Content.ToString();

            try
            {
                var srv = new driver.Service1Client();

                var module = new driver.CourseModule
                {
                    CourseId = currentModuleCourseId,
                    ModuleName = name,
                    Description = ModuleDescBox.Text?.Trim() ?? "",
                    ContentType = contentType,
                    DurationMinutes = duration,
                    OrderIndex = orderIndex,
                    IsRequired = IsRequiredCheck.IsChecked == true
                };

                if (editingModuleId.HasValue)
                {
                    module.ModuleId = editingModuleId.Value;
                    srv.UpdateModule(module);
                    MessageBox.Show("Module updated!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    srv.AddModule(module);
                    MessageBox.Show("Module added!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }

                ModuleFormOverlay.Visibility = Visibility.Collapsed;
                editingModuleId = null;
                LoadCourses();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving module: " + ex.Message, "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteModule_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag == null) return;

            int moduleId = Convert.ToInt32(button.Tag);

            var result = MessageBox.Show("Delete this module?", "Confirm Delete",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var srv = new driver.Service1Client();
                    srv.DeleteModule(moduleId);
                    LoadCourses();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting module: " + ex.Message, "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CancelModuleForm_Click(object sender, RoutedEventArgs e)
        {
            ModuleFormOverlay.Visibility = Visibility.Collapsed;
            editingModuleId = null;
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            page.Navigate(new TeacherUI());
        }
    }

    public class CourseListViewModel
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public string Description { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public List<ModuleListViewModel> Modules { get; set; }

        public int ModuleCount
        {
            get { return Modules != null ? Modules.Count : 0; }
        }

        public string StatusText
        {
            get { return IsActive ? "Active" : "Inactive"; }
        }

        public Brush StatusColor
        {
            get
            {
                return IsActive
                    ? new SolidColorBrush(Color.FromRgb(39, 174, 96))
                    : new SolidColorBrush(Color.FromRgb(231, 76, 60));
            }
        }

        public Visibility DeactivateVisibility
        {
            get { return IsActive ? Visibility.Visible : Visibility.Collapsed; }
        }
    }

    public class ModuleListViewModel
    {
        public int ModuleId { get; set; }
        public string ModuleName { get; set; }
        public string Description { get; set; }
        public string ContentType { get; set; }
        public int DurationMinutes { get; set; }
        public int OrderIndex { get; set; }
        public bool IsRequired { get; set; }

        public string RequiredText
        {
            get { return IsRequired ? "Required" : "Optional"; }
        }
    }
}
