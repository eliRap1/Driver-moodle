// StudentPricingManagement.xaml.cs
// This page allows teachers to set custom prices for specific students

using driver_client.driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace driver_client
{
    public partial class StudentPricingManagement : Page
    {
        private List<UserInfo> students = new List<UserInfo>();
        private UserInfo selectedStudent = null;

        public StudentPricingManagement()
        {
            InitializeComponent();
            LoadStudents();
        }

        private void LoadStudents()
        {
            try
            {
                var srv = new Service1Client();

                // Get all students for this teacher
                var allStudents = srv.GetAllUsers().ToList();
                students = allStudents.Where(s => s.TeacherId == LogIn.sign.Id).ToList();

                StudentsPanel.Children.Clear();

                if (students.Count == 0)
                {
                    StudentsPanel.Children.Add(new TextBlock
                    {
                        Text = "No students assigned to you yet.",
                        Foreground = Brushes.LightGray,
                        FontStyle = FontStyles.Italic,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(20)
                    });
                    return;
                }

                foreach (var student in students)
                {
                    StudentsPanel.Children.Add(CreateStudentCard(student));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading students: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private Border CreateStudentCard(UserInfo student)
        {
            // Determine the effective price
            int effectivePrice = student.CustomLessonPrice > 0
                ? student.CustomLessonPrice
                : LogIn.sign.LessonPrice;

            bool hasCustomPrice = student.CustomLessonPrice > 0;
            bool hasDiscount = student.DiscountPercent > 0;

            var stack = new StackPanel { Margin = new Thickness(5) };

            // Student name
            stack.Children.Add(new TextBlock
            {
                Text = $"👤 {student.Username}",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White
            });

            // Price info
            var priceText = $"Price: {effectivePrice} ₪";
            if (hasCustomPrice)
            {
                priceText += " (Custom)";
            }
            else if (hasDiscount)
            {
                priceText += $" ({student.DiscountPercent}% discount)";
            }
            else
            {
                priceText += " (Default)";
            }

            stack.Children.Add(new TextBlock
            {
                Text = priceText,
                FontSize = 12,
                Foreground = hasCustomPrice || hasDiscount
                    ? new SolidColorBrush(Color.FromRgb(0, 224, 255))
                    : Brushes.LightGray,
                Margin = new Thickness(0, 5, 0, 0)
            });

            var border = new Border
            {
                Background = selectedStudent?.Id == student.Id
                    ? new SolidColorBrush(Color.FromRgb(0, 100, 120))
                    : new SolidColorBrush(Color.FromRgb(26, 46, 80)),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(15),
                Margin = new Thickness(0, 0, 0, 10),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0, 192, 204)),
                BorderThickness = new Thickness(1),
                Child = stack,
                Cursor = System.Windows.Input.Cursors.Hand
            };

            border.MouseLeftButtonDown += (s, e) => SelectStudent(student);

            return border;
        }

        private void SelectStudent(UserInfo student)
        {
            selectedStudent = student;

            // Update UI
            LoadStudents(); // Refresh to show selection

            // Show edit panel
            EditPanel.Visibility = Visibility.Visible;
            SelectedStudentName.Text = $"Editing: {student.Username}";

            // Load current values
            CustomPriceBox.Text = student.CustomLessonPrice > 0
                ? student.CustomLessonPrice.ToString()
                : "";
            DiscountBox.Text = student.DiscountPercent > 0
                ? student.DiscountPercent.ToString()
                : "";

            // Set radio button
            if (student.CustomLessonPrice > 0)
            {
                rbCustomPrice.IsChecked = true;
            }
            else if (student.DiscountPercent > 0)
            {
                rbDiscount.IsChecked = true;
            }
            else
            {
                rbDefault.IsChecked = true;
            }
        }

        private void SaveStudentPrice_Click(object sender, RoutedEventArgs e)
        {
            if (selectedStudent == null)
            {
                MessageBox.Show("Please select a student first.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var srv = new Service1Client();

                if (rbDefault.IsChecked == true)
                {
                    // Reset to default
                    srv.SetStudentLessonPrice(selectedStudent.Id, 0);
                    srv.SetStudentDiscount(selectedStudent.Id, 0);
                    MessageBox.Show($"{selectedStudent.Username} will use your default price ({LogIn.sign.LessonPrice} ₪).",
                        "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else if (rbCustomPrice.IsChecked == true)
                {
                    // Set custom price
                    if (!int.TryParse(CustomPriceBox.Text, out int customPrice) || customPrice <= 0)
                    {
                        MessageBox.Show("Please enter a valid custom price.", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    srv.SetStudentLessonPrice(selectedStudent.Id, customPrice);
                    srv.SetStudentDiscount(selectedStudent.Id, 0); // Clear discount
                    MessageBox.Show($"{selectedStudent.Username}'s price set to {customPrice} ₪.",
                        "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else if (rbDiscount.IsChecked == true)
                {
                    // Set discount
                    if (!int.TryParse(DiscountBox.Text, out int discount) || discount < 1 || discount > 100)
                    {
                        MessageBox.Show("Please enter a valid discount (1-100%).", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    srv.SetStudentDiscount(selectedStudent.Id, discount);
                    srv.SetStudentLessonPrice(selectedStudent.Id, 0); // Clear custom price

                    int discountedPrice = LogIn.sign.LessonPrice - (LogIn.sign.LessonPrice * discount / 100);
                    MessageBox.Show($"{selectedStudent.Username} gets {discount}% discount.\nEffective price: {discountedPrice} ₪",
                        "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                // Refresh the list
                LoadStudents();
                EditPanel.Visibility = Visibility.Collapsed;
                selectedStudent = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving price: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyToAll_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "This will reset ALL students to your default price. Continue?",
                "Confirm Reset",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                var srv = new Service1Client();

                foreach (var student in students)
                {
                    srv.SetStudentLessonPrice(student.Id, 0);
                    srv.SetStudentDiscount(student.Id, 0);
                }

                MessageBox.Show($"All {students.Count} students reset to default price ({LogIn.sign.LessonPrice} ₪).",
                    "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                LoadStudents();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            page.Navigate(new TeacherSettings());
        }
    }
}
