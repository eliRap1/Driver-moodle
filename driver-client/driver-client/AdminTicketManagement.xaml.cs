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
    public partial class AdminTicketManagement : Page
    {
        private string currentFilter = "All";
        private DispatcherTimer refreshTimer;
        private List<SupportTicket> allTickets = new List<SupportTicket>();

        public AdminTicketManagement()
        {
            InitializeComponent();
            LoadTickets();

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
                allTickets = srv.GetAllTickets().ToList();

                FilterAndDisplayTickets();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading tickets:\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FilterAndDisplayTickets()
        {
            TicketsPanel.Children.Clear();

            var filteredTickets = currentFilter == "All"
                ? allTickets
                : allTickets.Where(t => t.Status == currentFilter).ToList();

            // Sort: Urgent first, then by date
            filteredTickets = filteredTickets
                .OrderBy(t => GetPriorityOrder(t.Priority))
                .ThenByDescending(t => t.CreatedAt)
                .ToList();

            if (filteredTickets.Count == 0)
            {
                TicketsPanel.Children.Add(new TextBlock
                {
                    Text = $"No {(currentFilter == "All" ? "" : currentFilter.ToLower())} tickets found.",
                    Foreground = Brushes.LightGray,
                    FontStyle = FontStyles.Italic,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 50, 0, 0)
                });
                return;
            }

            foreach (var ticket in filteredTickets)
            {
                TicketsPanel.Children.Add(CreateTicketCard(ticket));
            }

            UpdateFilterButtons();
        }

        private int GetPriorityOrder(string priority)
        {
            switch (priority)
            {
                case "Urgent": return 0;
                case "High": return 1;
                case "Medium": return 2;
                case "Low": return 3;
                default: return 4;
            }
        }

        private Border CreateTicketCard(SupportTicket ticket)
        {
            var mainStack = new StackPanel();

            // Header row
            var headerStack = new StackPanel { Orientation = Orientation.Horizontal };

            headerStack.Children.Add(new TextBlock
            {
                Text = $"#{ticket.TicketId}",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(0, 224, 255)),
                Margin = new Thickness(0, 0, 10, 0)
            });

            // Status badge
            headerStack.Children.Add(new Border
            {
                Background = GetStatusColor(ticket.Status),
                CornerRadius = new CornerRadius(3),
                Padding = new Thickness(6, 2, 6, 2),
                Margin = new Thickness(0, 0, 8, 0),
                Child = new TextBlock
                {
                    Text = ticket.Status,
                    Foreground = Brushes.White,
                    FontSize = 11,
                    FontWeight = FontWeights.Bold
                }
            });

            // Priority badge
            headerStack.Children.Add(new Border
            {
                Background = GetPriorityColor(ticket.Priority),
                CornerRadius = new CornerRadius(3),
                Padding = new Thickness(6, 2, 6, 2),
                Child = new TextBlock
                {
                    Text = ticket.Priority,
                    Foreground = Brushes.White,
                    FontSize = 11,
                    FontWeight = FontWeights.Bold
                }
            });

            // User type badge
            headerStack.Children.Add(new Border
            {
                Background = ticket.UserType == "Teacher"
                    ? new SolidColorBrush(Color.FromRgb(155, 89, 182))
                    : new SolidColorBrush(Color.FromRgb(52, 73, 94)),
                CornerRadius = new CornerRadius(3),
                Padding = new Thickness(6, 2, 6, 2),
                Margin = new Thickness(8, 0, 0, 0),
                Child = new TextBlock
                {
                    Text = ticket.UserType,
                    Foreground = Brushes.White,
                    FontSize = 11
                }
            });

            mainStack.Children.Add(headerStack);

            // Subject
            mainStack.Children.Add(new TextBlock
            {
                Text = ticket.Subject,
                FontSize = 15,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.White,
                Margin = new Thickness(0, 8, 0, 4),
                TextWrapping = TextWrapping.Wrap
            });

            // User info
            mainStack.Children.Add(new TextBlock
            {
                Text = $"From: {ticket.Username} • Created: {ticket.CreatedAt:dd/MM/yyyy HH:mm}",
                FontSize = 12,
                Foreground = Brushes.Gray
            });

            // Description preview
            string preview = ticket.Description.Length > 100
                ? ticket.Description.Substring(0, 100) + "..."
                : ticket.Description;

            mainStack.Children.Add(new TextBlock
            {
                Text = preview,
                FontSize = 13,
                Foreground = Brushes.LightGray,
                Margin = new Thickness(0, 6, 0, 10),
                TextWrapping = TextWrapping.Wrap
            });

            // Action buttons
            var buttonsStack = new StackPanel { Orientation = Orientation.Horizontal };

            // View/Respond button
            var viewBtn = new Button
            {
                Content = "View & Respond",
                Background = new SolidColorBrush(Color.FromRgb(0, 224, 255)),
                Foreground = Brushes.Black,
                FontWeight = FontWeights.Bold,
                Padding = new Thickness(12, 6, 12, 6),
                Margin = new Thickness(0, 0, 8, 0),
                Cursor = System.Windows.Input.Cursors.Hand,
                Tag = ticket
            };
            viewBtn.Click += ViewTicket_Click;
            buttonsStack.Children.Add(viewBtn);

            // Status change buttons based on current status
            if (ticket.Status == "Open")
            {
                var assignBtn = new Button
                {
                    Content = "Take Ticket",
                    Background = new SolidColorBrush(Color.FromRgb(241, 196, 15)),
                    Foreground = Brushes.Black,
                    FontWeight = FontWeights.Bold,
                    Padding = new Thickness(12, 6, 12, 6),
                    Margin = new Thickness(0, 0, 8, 0),
                    Cursor = System.Windows.Input.Cursors.Hand,
                    Tag = ticket
                };
                assignBtn.Click += TakeTicket_Click;
                buttonsStack.Children.Add(assignBtn);
            }

            if (ticket.Status == "In Progress")
            {
                var resolveBtn = new Button
                {
                    Content = "Mark Resolved",
                    Background = new SolidColorBrush(Color.FromRgb(46, 204, 113)),
                    Foreground = Brushes.White,
                    FontWeight = FontWeights.Bold,
                    Padding = new Thickness(12, 6, 12, 6),
                    Margin = new Thickness(0, 0, 8, 0),
                    Cursor = System.Windows.Input.Cursors.Hand,
                    Tag = ticket
                };
                resolveBtn.Click += ResolveTicket_Click;
                buttonsStack.Children.Add(resolveBtn);
            }

            if (ticket.Status == "Resolved")
            {
                var closeBtn = new Button
                {
                    Content = "Close Ticket",
                    Background = new SolidColorBrush(Color.FromRgb(149, 165, 166)),
                    Foreground = Brushes.White,
                    FontWeight = FontWeights.Bold,
                    Padding = new Thickness(12, 6, 12, 6),
                    Cursor = System.Windows.Input.Cursors.Hand,
                    Tag = ticket
                };
                closeBtn.Click += CloseTicket_Click;
                buttonsStack.Children.Add(closeBtn);
            }

            mainStack.Children.Add(buttonsStack);

            return new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(27, 45, 71)),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(15),
                Margin = new Thickness(0, 0, 0, 12),
                BorderBrush = ticket.Priority == "Urgent"
                    ? new SolidColorBrush(Color.FromRgb(231, 76, 60))
                    : new SolidColorBrush(Color.FromRgb(0, 192, 204)),
                BorderThickness = new Thickness(ticket.Priority == "Urgent" ? 2 : 1),
                Child = mainStack
            };
        }

        private void ViewTicket_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is SupportTicket ticket)
            {
                refreshTimer.Stop();
                page.Navigate(new AdminTicketDetails(ticket.TicketId));
            }
        }

        private void TakeTicket_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is SupportTicket ticket)
            {
                try
                {
                    var srv = new Service1Client();
                    srv.UpdateTicketStatus(ticket.TicketId, "In Progress", LogIn.sign.Username);
                    MessageBox.Show($"Ticket #{ticket.TicketId} assigned to you.", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadTickets();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error:\n{ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ResolveTicket_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is SupportTicket ticket)
            {
                try
                {
                    var srv = new Service1Client();
                    srv.UpdateTicketStatus(ticket.TicketId, "Resolved", ticket.AssignedTo);
                    MessageBox.Show($"Ticket #{ticket.TicketId} marked as resolved.", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadTickets();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error:\n{ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CloseTicket_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is SupportTicket ticket)
            {
                try
                {
                    var srv = new Service1Client();
                    srv.CloseTicket(ticket.TicketId, "Closed by admin", null);
                    MessageBox.Show($"Ticket #{ticket.TicketId} closed.", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadTickets();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error:\n{ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private Brush GetStatusColor(string status)
        {
            switch (status)
            {
                case "Open": return new SolidColorBrush(Color.FromRgb(52, 152, 219));
                case "In Progress": return new SolidColorBrush(Color.FromRgb(241, 196, 15));
                case "Resolved": return new SolidColorBrush(Color.FromRgb(46, 204, 113));
                case "Closed": return new SolidColorBrush(Color.FromRgb(149, 165, 166));
                default: return Brushes.Gray;
            }
        }

        private Brush GetPriorityColor(string priority)
        {
            switch (priority)
            {
                case "Low": return new SolidColorBrush(Color.FromRgb(46, 204, 113));
                case "Medium": return new SolidColorBrush(Color.FromRgb(243, 156, 18));
                case "High": return new SolidColorBrush(Color.FromRgb(230, 126, 34));
                case "Urgent": return new SolidColorBrush(Color.FromRgb(231, 76, 60));
                default: return Brushes.Gray;
            }
        }

        private void UpdateFilterButtons()
        {
            var buttons = new[] { AllBtn, OpenBtn, InProgressBtn, ResolvedBtn, ClosedBtn };
            var filters = new[] { "All", "Open", "In Progress", "Resolved", "Closed" };

            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].Background = filters[i] == currentFilter
                    ? new SolidColorBrush(Color.FromRgb(0, 224, 255))
                    : new SolidColorBrush(Color.FromRgb(27, 45, 71));
                buttons[i].Foreground = filters[i] == currentFilter ? Brushes.Black : Brushes.White;
            }
        }

        private void FilterAll_Click(object sender, RoutedEventArgs e)
        {
            currentFilter = "All";
            FilterAndDisplayTickets();
        }

        private void FilterOpen_Click(object sender, RoutedEventArgs e)
        {
            currentFilter = "Open";
            FilterAndDisplayTickets();
        }

        private void FilterInProgress_Click(object sender, RoutedEventArgs e)
        {
            currentFilter = "In Progress";
            FilterAndDisplayTickets();
        }

        private void FilterResolved_Click(object sender, RoutedEventArgs e)
        {
            currentFilter = "Resolved";
            FilterAndDisplayTickets();
        }

        private void FilterClosed_Click(object sender, RoutedEventArgs e)
        {
            currentFilter = "Closed";
            FilterAndDisplayTickets();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            refreshTimer.Stop();
            page.Navigate(new AdminDashboard());
        }
    }
}
