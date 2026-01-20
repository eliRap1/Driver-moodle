using driver_client.driver;
using System;
using System.Windows;
using System.Windows.Controls;

namespace driver_client
{
    public partial class CreateTicket : Page
    {
        public CreateTicket()
        {
            InitializeComponent();
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            string subject = SubjectBox.Text.Trim();
            string description = DescriptionBox.Text.Trim();

            // Validation
            if (string.IsNullOrEmpty(subject))
            {
                MessageBox.Show("Please enter a subject.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(description))
            {
                MessageBox.Show("Please enter a description.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (subject.Length < 5)
            {
                MessageBox.Show("Subject must be at least 5 characters.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (description.Length < 10)
            {
                MessageBox.Show("Description must be at least 10 characters.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var srv = new Service1Client();

                string priority = ((ComboBoxItem)PriorityCombo.SelectedItem).Content.ToString();

                var ticket = new SupportTicket
                {
                    UserId = LogIn.sign.Id,
                    Username = LogIn.sign.Username,
                    UserType = LogIn.sign.IsTeacher ? "Teacher" : "Student",
                    Subject = subject,
                    Description = description,
                    Priority = priority,
                    Status = "Open",
                    CreatedAt = DateTime.Now
                };

                int ticketId = srv.CreateSupportTicket(ticket);

                if (ticketId > 0)
                {
                    MessageBox.Show($"Support ticket #{ticketId} created successfully!\n\n" +
                                  "Our team will review your request and respond soon.",
                                  "Ticket Created",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Information);

                    // Navigate to tickets list
                    page.Navigate(new MyTickets());
                }
                else
                {
                    MessageBox.Show("Failed to create ticket. Please try again.",
                                  "Error",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"DETAILED ERROR:\n\n" +
                               $"Message: {ex.Message}\n\n" +
                               $"Inner: {ex.InnerException?.Message}\n\n" +
                               $"Stack: {ex.StackTrace}",
                               "Error Details",
                               MessageBoxButton.OK,
                               MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Cancel ticket submission?",
                "Confirm",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                if (LogIn.sign.IsTeacher)
                    page.Navigate(new TeacherUI());
                else
                    page.Navigate(new StudentUI());
            }
        }
    }
}