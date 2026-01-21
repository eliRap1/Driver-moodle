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
    public partial class TicketDetails : Page
    {
        private int ticketId;
        private SupportTicket currentTicket;
        private DispatcherTimer refreshTimer;

        public TicketDetails(int ticketId)
        {
            InitializeComponent();
            this.ticketId = ticketId;
            LoadTicketDetails();

            // Auto-refresh messages every 10 seconds
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

                // Status
                StatusText.Text = currentTicket.Status;
                StatusBadge.Background = GetStatusColor(currentTicket.Status);

                // Priority
                PriorityText.Text = currentTicket.Priority;
                PriorityBadge.Background = GetPriorityColor(currentTicket.Priority);

                // Dates
                CreatedText.Text = currentTicket.CreatedAt.ToString("dd/MM/yyyy HH:mm");

                if (currentTicket.UpdatedAt.HasValue)
                {
                    UpdatedLabel.Visibility = Visibility.Visible;
                    UpdatedText.Visibility = Visibility.Visible;
                    UpdatedText.Text = currentTicket.UpdatedAt.Value.ToString("dd/MM/yyyy HH:mm");
                }

                if (!string.IsNullOrEmpty(currentTicket.AssignedTo))
                {
                    AssignedLabel.Visibility = Visibility.Visible;
                    AssignedText.Visibility = Visibility.Visible;
                    AssignedText.Text = currentTicket.AssignedTo;
                }

                // Load messages
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

                // Scroll to bottom
                MessagesScroll.ScrollToBottom();
            }
            catch (Exception ex)
            {
                // Silent fail for refresh errors
                System.Diagnostics.Debug.WriteLine($"Error loading messages: {ex.Message}");
            }
        }

        private Border CreateMessageBubble(TicketMessage message)
        {
            bool isFromMe = message.SenderUsername == LogIn.sign.Username;

            var stack = new StackPanel();

            // Sender name
            stack.Children.Add(new TextBlock
            {
                Text = message.IsAdmin ? $"🛡️ {message.SenderUsername} (Support)" : message.SenderUsername,
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Foreground = message.IsAdmin ? new SolidColorBrush(Color.FromRgb(255, 215, 0)) : Brushes.White,
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
                Background = message.IsAdmin
                    ? new SolidColorBrush(Color.FromRgb(70, 50, 100))  // Purple for admin
                    : isFromMe
                        ? new SolidColorBrush(Color.FromRgb(0, 224, 255))  // Cyan for user
                        : new SolidColorBrush(Color.FromRgb(45, 45, 48)),  // Dark for others
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(12),
                Margin = new Thickness(isFromMe ? 50 : 0, 5, isFromMe ? 0 : 50, 5),
                HorizontalAlignment = isFromMe ? HorizontalAlignment.Right : HorizontalAlignment.Left,
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

            if (message.Length < 3)
            {
                MessageBox.Show("Message must be at least 3 characters.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                driver.Service1Client srv = new driver.Service1Client();

                var ticketMessage = new TicketMessage
                {
                    TicketId = ticketId,
                    SenderUsername = LogIn.sign.Username,
                    IsAdmin = false,  // Regular users are not admins
                    Message = message,
                    SentAt = DateTime.Now
                };

                srv.AddTicketMessage(ticketMessage);

                ReplyBox.Clear();
                LoadMessages();

                MessageBox.Show("Reply sent successfully!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending reply:\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private Brush GetStatusColor(string status)
        {
            switch (status)
            {
                case "Open":
                    return new SolidColorBrush(Color.FromRgb(52, 152, 219));
                case "In Progress":
                    return new SolidColorBrush(Color.FromRgb(241, 196, 15));
                case "Resolved":
                    return new SolidColorBrush(Color.FromRgb(46, 204, 113));
                case "Closed":
                    return new SolidColorBrush(Color.FromRgb(149, 165, 166));
                default:
                    return Brushes.Gray;
            }
        }

        private Brush GetPriorityColor(string priority)
        {
            switch (priority)
            {
                case "Low":
                    return new SolidColorBrush(Color.FromRgb(46, 204, 113));
                case "Medium":
                    return new SolidColorBrush(Color.FromRgb(243, 156, 18));
                case "High":
                    return new SolidColorBrush(Color.FromRgb(230, 126, 34));
                case "Urgent":
                    return new SolidColorBrush(Color.FromRgb(231, 76, 60));
                default:
                    return Brushes.Gray;
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            refreshTimer.Stop();
            page.Navigate(new MyTickets());
        }
    }
}