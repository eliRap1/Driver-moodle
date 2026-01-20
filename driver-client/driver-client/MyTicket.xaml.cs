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
    public partial class MyTickets : Page
    {
        private DispatcherTimer refreshTimer;

        public MyTickets()
        {
            InitializeComponent();
            LoadTickets();

            // Auto-refresh every 30 seconds
            refreshTimer = new DispatcherTimer();
            refreshTimer.Interval = TimeSpan.FromSeconds(30);
            refreshTimer.Tick += (s, e) => LoadTickets();
            refreshTimer.Start();
        }

        private void LoadTickets()
        {
            try
            {
                var srv = new Service1Client();
                List<SupportTicket> tickets = srv.GetUserTickets(LogIn.sign.Id).ToList();

                TicketsPanel.Children.Clear();

                if (tickets.Count == 0)
                {
                    var emptyMessage = new TextBlock
                    {
                        Text = "No tickets found. Click '+ New Ticket' to create one.",
                        FontSize = 16,
                        Foreground = Brushes.LightGray,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 50, 0, 0)
                    };
                    TicketsPanel.Children.Add(emptyMessage);
                    return;
                }

                // Sort: Open tickets first, then by date
                tickets = tickets.OrderBy(t => t.Status == "Closed" ? 1 : 0)
                                .ThenByDescending(t => t.CreatedAt)
                                .ToList();

                foreach (var ticket in tickets)
                {
                    TicketsPanel.Children.Add(CreateTicketCard(ticket));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading tickets:\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private Border CreateTicketCard(SupportTicket ticket)
        {
            var stack = new StackPanel();

            // Header with ticket ID and status
            var header = new StackPanel { Orientation = Orientation.Horizontal };

            header.Children.Add(new TextBlock
            {
                Text = $"Ticket #{ticket.TicketId}",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                Margin = new Thickness(0, 0, 15, 0)
            });

            header.Children.Add(new Border
            {
                Background = GetStatusColor(ticket.Status),
                CornerRadius = new CornerRadius(3),
                Padding = new Thickness(8, 4, 8, 4),
                Child = new TextBlock
                {
                    Text = ticket.Status,
                    Foreground = Brushes.White,
                    FontSize = 12,
                    FontWeight = FontWeights.Bold
                }
            });

            // Priority badge
            header.Children.Add(new Border
            {
                Background = GetPriorityColor(ticket.Priority),
                CornerRadius = new CornerRadius(3),
                Padding = new Thickness(8, 4, 8, 4),
                Margin = new Thickness(8, 0, 0, 0),
                Child = new TextBlock
                {
                    Text = ticket.Priority,
                    Foreground = Brushes.White,
                    FontSize = 12,
                    FontWeight = FontWeights.Bold
                }
            });

            stack.Children.Add(header);

            // Subject
            stack.Children.Add(new TextBlock
            {
                Text = ticket.Subject,
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(0, 224, 255)),
                Margin = new Thickness(0, 8, 0, 4),
                TextWrapping = TextWrapping.Wrap
            });

            // Description preview
            string preview = ticket.Description.Length > 150
                ? ticket.Description.Substring(0, 150) + "..."
                : ticket.Description;

            stack.Children.Add(new TextBlock
            {
                Text = preview,
                FontSize = 14,
                Foreground = Brushes.LightGray,
                Margin = new Thickness(0, 4, 0, 8),
                TextWrapping = TextWrapping.Wrap
            });

            // Footer with dates
            var footer = new StackPanel();

            footer.Children.Add(new TextBlock
            {
                Text = $"Created: {ticket.CreatedAt:dd/MM/yyyy HH:mm}",
                FontSize = 12,
                Foreground = Brushes.Gray,
                Margin = new Thickness(0, 4, 0, 2)
            });

            if (ticket.UpdatedAt.HasValue)
            {
                footer.Children.Add(new TextBlock
                {
                    Text = $"Last Updated: {ticket.UpdatedAt.Value:dd/MM/yyyy HH:mm}",
                    FontSize = 12,
                    Foreground = Brushes.Gray,
                    Margin = new Thickness(0, 2, 0, 2)
                });
            }

            if (!string.IsNullOrEmpty(ticket.AssignedTo))
            {
                footer.Children.Add(new TextBlock
                {
                    Text = $"Assigned to: {ticket.AssignedTo}",
                    FontSize = 12,
                    Foreground = new SolidColorBrush(Color.FromRgb(0, 200, 200)),
                    Margin = new Thickness(0, 2, 0, 2)
                });
            }

            stack.Children.Add(footer);

            // View Details Button
            var viewButton = new Button
            {
                Content = "View Details",
                Tag = ticket,
                Background = new SolidColorBrush(Color.FromRgb(0, 224, 255)),
                Foreground = Brushes.Black,
                BorderBrush = Brushes.Transparent,
                FontWeight = FontWeights.Bold,
                Width = 120,
                Height = 35,
                Margin = new Thickness(0, 10, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left,
                Cursor = System.Windows.Input.Cursors.Hand
            };
            viewButton.Click += ViewDetails_Click;
            stack.Children.Add(viewButton);

            return new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(26, 46, 80)),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(15),
                Margin = new Thickness(0, 0, 0, 15),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0, 224, 255)),
                BorderThickness = new Thickness(1),
                Child = stack,
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Colors.Black,
                    Opacity = 0.3,
                    BlurRadius = 8,
                    ShadowDepth = 2
                }
            };
        }

        private Brush GetStatusColor(string status)
        {
            switch (status)
            {
                case "Open":
                    return new SolidColorBrush(Color.FromRgb(52, 152, 219)); // Blue
                case "In Progress":
                    return new SolidColorBrush(Color.FromRgb(241, 196, 15)); // Yellow
                case "Resolved":
                    return new SolidColorBrush(Color.FromRgb(46, 204, 113)); // Green
                case "Closed":
                    return new SolidColorBrush(Color.FromRgb(149, 165, 166)); // Gray
                default:
                    return Brushes.Gray;
            }
        }

        private Brush GetPriorityColor(string priority)
        {
            switch (priority)
            {
                case "Low":
                    return new SolidColorBrush(Color.FromRgb(46, 204, 113)); // Green
                case "Medium":
                    return new SolidColorBrush(Color.FromRgb(243, 156, 18)); // Orange
                case "High":
                    return new SolidColorBrush(Color.FromRgb(230, 126, 34)); // Dark Orange
                case "Urgent":
                    return new SolidColorBrush(Color.FromRgb(231, 76, 60)); // Red
                default:
                    return Brushes.Gray;
            }
        }

        private void ViewDetails_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is SupportTicket ticket)
            {
                refreshTimer.Stop();
                page.Navigate(new TicketDetails(ticket.TicketId));
            }
        }

        private void NewTicket_Click(object sender, RoutedEventArgs e)
        {
            refreshTimer.Stop();
            page.Navigate(new CreateTicket());
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            refreshTimer.Stop();
            if (LogIn.sign.IsTeacher)
                page.Navigate(new TeacherUI());
            else
                page.Navigate(new StudentUI());
        }
    }
}