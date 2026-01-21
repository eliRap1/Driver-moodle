using driver_client.driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace driver_client
{
    public partial class AdminTicketDetails : Page
    {
        private int ticketId;
        private SupportTicket currentTicket;
        private DispatcherTimer refreshTimer;

        public AdminTicketDetails(int ticketId)
        {
            InitializeComponent();
            this.ticketId = ticketId;
            LoadTicketDetails();

            refreshTimer = new DispatcherTimer();
            refreshTimer.Interval = TimeSpan.FromSeconds(10);
            refreshTimer.Tick += (s, e) => LoadMessages();
            refreshTimer.Start();
        }

        private void LoadTicketDetails()
        {
            try
            {
                var srv = new Service1Client();
                currentTicket = srv.GetTicketById(ticketId);

                if (currentTicket == null)
                {
                    MessageBox.Show("Ticket not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Back_Click(null, null);
                    return;
                }

                // Header
                TicketTitle.Text = $"Ticket #{currentTicket.TicketId}";
                TicketSubject.Text = currentTicket.Subject;
                TicketDescription.Text = currentTicket.Description;

                // User info
                UserTypeIcon.Text = currentTicket.UserType == "Teacher" ? "👨‍🏫" : "👤";
                UsernameText.Text = $"{currentTicket.Username} ({currentTicket.UserType})";

                // Status combo
                for (int i = 0; i < StatusCombo.Items.Count; i++)
                {
                    if ((StatusCombo.Items[i] as ComboBoxItem)?.Content.ToString() == currentTicket.Status)
                    {
                        StatusCombo.SelectedIndex = i;
                        break;
                    }
                }

                // Priority combo
                for (int i = 0; i < PriorityCombo.Items.Count; i++)
                {
                    if ((PriorityCombo.Items[i] as ComboBoxItem)?.Content.ToString() == currentTicket.Priority)
                    {
                        PriorityCombo.SelectedIndex = i;
                        break;
                    }
                }

                // Assigned
                AssignedText.Text = string.IsNullOrEmpty(currentTicket.AssignedTo)
                    ? "Unassigned"
                    : currentTicket.AssignedTo;

                AssignToMeBtn.Visibility = currentTicket.AssignedTo == LogIn.sign.Username
                    ? Visibility.Collapsed
                    : Visibility.Visible;

                // Dates
                CreatedText.Text = currentTicket.CreatedAt.ToString("dd/MM/yyyy HH:mm");
                UpdatedText.Text = currentTicket.UpdatedAt.HasValue
                    ? currentTicket.UpdatedAt.Value.ToString("dd/MM/yyyy HH:mm")
                    : "N/A";

                LoadMessages();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading ticket:\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadMessages()
        {
            try
            {
                var srv = new Service1Client();
                List<TicketMessage> messages = srv.GetTicketMessages(ticketId).ToList();

                MessagesPanel.Children.Clear();

                foreach (var message in messages)
                {
                    MessagesPanel.Children.Add(CreateMessageBubble(message));
                }

                MessagesScroll.ScrollToBottom();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading messages: {ex.Message}");
            }
        }

        private Border CreateMessageBubble(TicketMessage message)
        {
            bool isAdmin = message.IsAdmin;

            var stack = new StackPanel();

            // Sender name
            stack.Children.Add(new TextBlock
            {
                Text = isAdmin ? $"🛡️ {message.SenderUsername} (Admin)" : message.SenderUsername,
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Foreground = isAdmin ? Brushes.Gold : Brushes.White,
                Margin = new Thickness(0, 0, 0, 4)
            });

            // Message text
            stack.Children.Add(new TextBlock
            {
                Text = message.Message,
                FontSize = 14,
                Foreground = Brushes.White,
                TextWrapping = TextWrapping.Wrap
            });

            // Timestamp
            stack.Children.Add(new TextBlock
            {
                Text = message.SentAt.ToString("dd/MM/yyyy HH:mm"),
                FontSize = 11,
                Foreground = Brushes.Gray,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 4, 0, 0)
            });

            return new Border
            {
                Background = isAdmin
                    ? new SolidColorBrush(Color.FromRgb(70, 50, 100))  // Purple for admin
                    : new SolidColorBrush(Color.FromRgb(45, 45, 48)),  // Dark for user
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(12),
                Margin = new Thickness(isAdmin ? 50 : 0, 5, isAdmin ? 0 : 50, 5),
                HorizontalAlignment = isAdmin ? HorizontalAlignment.Right : HorizontalAlignment.Left,
                Child = stack,
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Colors.Black,
                    Opacity = 0.3,
                    BlurRadius = 6,
                    ShadowDepth = 2
                }
            };
        }

        private void SendReply_Click(object sender, RoutedEventArgs e)
        {
            string message = ReplyBox.Text.Trim();

            if (string.IsNullOrEmpty(message))
            {
                MessageBox.Show("Please enter a message.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var srv = new Service1Client();

                var ticketMessage = new TicketMessage
                {
                    TicketId = ticketId,
                    SenderUsername = LogIn.sign.Username,
                    IsAdmin = true,  // Admin reply
                    Message = message,
                    SentAt = DateTime.Now
                };

                srv.AddTicketMessage(ticketMessage);

                // Auto-assign if not assigned
                if (string.IsNullOrEmpty(currentTicket.AssignedTo))
                {
                    srv.UpdateTicketStatus(ticketId, "In Progress", LogIn.sign.Username);
                }

                ReplyBox.Clear();
                LoadMessages();
                LoadTicketDetails(); // Refresh to update assigned

                MessageBox.Show("Reply sent!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending reply:\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StatusCombo_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (currentTicket == null || StatusCombo.SelectedItem == null) return;

            string newStatus = (StatusCombo.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (newStatus == currentTicket.Status) return;

            try
            {
                var srv = new Service1Client();
                srv.UpdateTicketStatus(ticketId, newStatus, currentTicket.AssignedTo ?? LogIn.sign.Username);
                currentTicket.Status = newStatus;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating status:\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PriorityCombo_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (currentTicket == null || PriorityCombo.SelectedItem == null) return;

            string newPriority = (PriorityCombo.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (newPriority == currentTicket.Priority) return;

            try
            {
                var srv = new Service1Client();
                // You would need to add UpdateTicketPriority to IService1
                // For now we'll just update locally
                currentTicket.Priority = newPriority;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating priority:\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AssignToMe_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var srv = new Service1Client();
                srv.UpdateTicketStatus(ticketId, "In Progress", LogIn.sign.Username);
                LoadTicketDetails();
                MessageBox.Show("Ticket assigned to you.", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error:\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResolveAndClose_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to resolve and close this ticket?",
                "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                var srv = new Service1Client();
                srv.CloseTicket(ticketId, "Resolved and closed by admin", LogIn.sign.Username);
                MessageBox.Show("Ticket resolved and closed.", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                Back_Click(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error:\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            refreshTimer.Stop();
            page.Navigate(new AdminTicketManagement());
        }
    }
}
