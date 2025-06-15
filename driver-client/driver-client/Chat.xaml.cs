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
    public partial class Chat : Page
    {
        private DispatcherTimer RefreshTimer;
        public Chat()
        {
            InitializeComponent();
            RefreshMessages(null, null);
            RefreshTimer = new DispatcherTimer(); // POOLING THREAD 
            RefreshTimer.Interval = TimeSpan.FromSeconds(3);
            RefreshTimer.Tick += RefreshMessages;
            RefreshTimer.Start();
        }
        private void RefreshMessages(object sender, EventArgs e)
        {
            driver.Service1Client srv = new driver.Service1Client();
            List<Chats> messages = srv.GetAllChat().ToList();
            MessagesPanel.Children.Clear();
            foreach (var message in messages)
            {
                AddMessage(message.Username, message.Message, message.IsTeacher, message.Time);
            }
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            page.Navigate(new StudentUI());
        }
        private void Send_Click(object sender, RoutedEventArgs e)
        {
            string message = MessageTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(message))
            {
                driver.Service1Client srv = new driver.Service1Client();
                srv.AddMessage(message, LogIn.sign.Id, LogIn.sign.Username, LogIn.sign.IsTeacher);
                AddMessage(LogIn.sign.Username, message, LogIn.sign.IsTeacher, DateTime.Now);
                MessageTextBox.Clear();
            }
        }

        public void AddMessage(string sender, string message, bool isTeacher, DateTime sendTime)
        {
            bool isMe = sender == LogIn.sign.Username;

            var senderBlock = new TextBlock
            {
                Text = isTeacher ? $"👨‍🏫 {sender}" : sender,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                FontSize = 17,
                Margin = new Thickness(0, 0, 0, 4)
            };

            // Message
            var messageBlock = new TextBlock
            {
                Text = message,
                TextWrapping = TextWrapping.Wrap,
                FontSize = 18,
                Foreground = Brushes.White,
                FontWeight = FontWeights.SemiBold
            };

            // Timestamp
            var timeBlock = new TextBlock
            {
                Text = sendTime.ToShortTimeString(),
                FontSize = 12,
                Foreground = Brushes.LightGray,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 4, 0, 0)
            };

            // StackPanel to hold the three text elements
            var stack = new StackPanel();
            stack.Children.Add(senderBlock);
            stack.Children.Add(messageBlock);
            stack.Children.Add(timeBlock);

            // Border to wrap the message nicely
            var border = new Border
            {
                Background = isMe
                    ? new SolidColorBrush(Color.FromRgb(0, 224, 255)) // bright blue for self
                    : new SolidColorBrush(Color.FromRgb(45, 45, 48)), // dark grey for others

                CornerRadius = new CornerRadius(18),
                Padding = new Thickness(16),
                Margin = new Thickness(12),
                MaxWidth = 500,
                HorizontalAlignment = isMe ? HorizontalAlignment.Right : HorizontalAlignment.Left,
                Child = stack,
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Colors.Black,
                    Opacity = 0.2,
                    BlurRadius = 8
                }
            };

            MessagesPanel.Children.Add(border);
        }



    }
}
