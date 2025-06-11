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
using System.Windows.Threading;

namespace driver_client
{
    /// <summary>
    /// Interaction logic for Rewiews.xaml
    /// </summary>
    public partial class Rewiews : Page
    {
        private DispatcherTimer updateRewiews;
        private int teacherId;
        public bool r;
        public Rewiews(int teacherId, bool rewiew = true)
        {
            InitializeComponent();
            this.teacherId = teacherId;
            LoadReviews(null, null);
            updateRewiews = new DispatcherTimer(); // POOLING THREAD 
            updateRewiews.Interval = TimeSpan.FromSeconds(5);
            updateRewiews.Tick += LoadReviews;
            updateRewiews.Start();
            r= rewiew;
        }

        private void LoadReviews(object sender, EventArgs e)
        {
            driver.Service1Client srv = new driver.Service1Client();
            var reviews = srv.GetTeacherReviews(teacherId); 

            foreach (var review in reviews)
            {
                var card = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(26, 46, 80)),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(0, 224, 255)),
                    BorderThickness = new Thickness(1.5),
                    CornerRadius = new CornerRadius(8),
                    Margin = new Thickness(10),
                    Padding = new Thickness(12),
                    Child = new TextBlock
                    {
                        Text = review,
                        Foreground = Brushes.White,
                        FontSize = 14,
                        TextWrapping = TextWrapping.Wrap
                    }
                };

                ReviewList.Children.Add(card);
            }

            if (!reviews.Any())
            {
                ReviewList.Children.Add(new TextBlock
                {
                    Text = "No reviews yet.",
                    Foreground = Brushes.LightGray,
                    FontSize = 16,
                    Margin = new Thickness(20),
                    HorizontalAlignment = HorizontalAlignment.Center
                });
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            updateRewiews.Stop();
            page.Navigate(new ChooseTeacher(r));
        }
    }

}
