using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace driver_client
{
    public partial class Chat : Page
    {
        public Chat()
        {
            InitializeComponent();

            // אפשר להכניס כאן טעינה של הודעות קיימות אם צריך
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            string message = MessageTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(message))
            {
                //AddMessage(Login.user, message);
                MessageTextBox.Clear();

                // שלח לשרת את ההודעה כאן אם צריך
            }
        }

        public void AddMessage(string sender, string message)
        {
            var messageBlock = new TextBlock
            {
                Text = $"{sender}: {message}",
                Foreground = Brushes.White,
                Background = new SolidColorBrush(Color.FromArgb(40, 0, 224, 255)),
                Padding = new Thickness(10),
                Margin = new Thickness(5),
                TextWrapping = TextWrapping.Wrap,
                FontSize = 14
            };

            MessagesPanel.Children.Add(messageBlock);
        }
    }
}
