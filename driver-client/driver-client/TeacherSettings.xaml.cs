// TeacherSettings.xaml.cs

using driver_client.driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace driver_client
{
    public partial class TeacherSettings : Page
    {
        public TeacherSettings()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            try
            {
                // Always pull fresh values from server so the UI reflects DB state,
                // not stale Sign defaults.
                int teacherId = ClientSession.TeacherId;
                var teacher = ServiceGateway.Use(client => client.GetUserById(teacherId, "Teacher"));

                int price = (teacher != null && teacher.LessonPrice > 0) ? teacher.LessonPrice : 200;
                string paymentMethods = teacher?.PaymentMethods ?? "Cash,Credit Card,Bank Transfer";

                // Sync Sign cache so other pages see fresh values too.
                LogIn.sign.LessonPrice = price;
                LogIn.sign.PaymentMethods = paymentMethods;

                LessonPriceBox.Text = price.ToString();

                string[] methods = paymentMethods.Split(',');
                chkCash.IsChecked = methods.Contains("Cash");
                chkCreditCard.IsChecked = methods.Contains("Credit Card");
                chkBankTransfer.IsChecked = methods.Contains("Bank Transfer");
                chkBit.IsChecked = methods.Contains("Bit");
                chkPaybox.IsChecked = methods.Contains("Paybox");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading settings: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate and update lesson price
                if (!int.TryParse(LessonPriceBox.Text, out int price) || price < 0)
                {
                    MessageBox.Show("Please enter a valid lesson price.", "Validation Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (price > 10000)
                {
                    MessageBox.Show("Lesson price seems too high. Maximum is 10,000 ₪.", "Validation Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Update lesson price
                ServiceGateway.Use(client => client.UpdateLessonPrice(ClientSession.TeacherId, price));
                LogIn.sign.LessonPrice = price;

                // Collect selected payment methods
                var methods = new List<string>();
                if (chkCash.IsChecked == true) methods.Add("Cash");
                if (chkCreditCard.IsChecked == true) methods.Add("Credit Card");
                if (chkBankTransfer.IsChecked == true) methods.Add("Bank Transfer");
                if (chkBit.IsChecked == true) methods.Add("Bit");
                if (chkPaybox.IsChecked == true) methods.Add("Paybox");

                if (methods.Count == 0)
                {
                    MessageBox.Show("Please select at least one payment method.", "Validation Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Update payment methods
                string paymentMethods = string.Join(",", methods);
                ServiceGateway.Use(client => client.UpdatePaymentMethods(ClientSession.TeacherId, paymentMethods));
                LogIn.sign.PaymentMethods = paymentMethods;

                MessageBox.Show("Settings saved successfully! ✓", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ManageStudentPrices_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to student pricing management page
            page.Navigate(new StudentPricingManagement());
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            page.Navigate(new TeacherUI());
        }
    }
}
