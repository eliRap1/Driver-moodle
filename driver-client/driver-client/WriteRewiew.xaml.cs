using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace driver_client
{
    /// <summary>
    /// Interaction logic for WriteRewiew.xaml
    /// </summary>
    public partial class WriteRewiew : Page
    {
        private int selectedRating = 0;

        public WriteRewiew()
        {
            InitializeComponent();
        }

        private void Star_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button starButton)
            {
                int rating = int.Parse(starButton.Tag.ToString());
                selectedRating = rating;

                StackPanel starPanel = (StackPanel)starButton.Parent;
                for (int i = 0; i < starPanel.Children.Count; i++)
                {
                    Button btn = (Button)starPanel.Children[i];
                    btn.Foreground = (i < rating) ? Brushes.Gold : Brushes.Gray;
                }
            }
        }

        private void SubmitReview_Click(object sender, RoutedEventArgs e)
        {
            string review = reviewText.Text.Trim();

            if (selectedRating == 0)
            {
                MessageBox.Show("Please select a rating.", "Missing Rating", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(review))
            {
                MessageBox.Show("Please enter your review.", "Missing Review", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                driver.Service1Client client = new driver.Service1Client();
                int tid = client.GetTeacherId(LogIn.sign.Id);
                client.UpdateRating(tid, selectedRating, review);

                MessageBox.Show("Thank you for your review!", "Submitted", MessageBoxButton.OK, MessageBoxImage.Information);
                StudentUI.madeRewiew = true;
                page.Navigate(new StudentUI());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error submitting review: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            page.Navigate(new StudentUI());
        }
    }
}
