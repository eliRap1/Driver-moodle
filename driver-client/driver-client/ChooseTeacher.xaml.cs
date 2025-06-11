using driver_client.driver;
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
    /// Interaction logic for ChooseTeacher.xaml
    /// </summary>
    public partial class ChooseTeacher : Page
    {
        private List<UserInfo> teachers;
        private bool chooseMode;
        private DispatcherTimer updateTeachers;

        public ChooseTeacher(bool chooseTeacher)
        {
            InitializeComponent();
            chooseMode = chooseTeacher;
            LoadTeachers(null, null);
            updateTeachers = new DispatcherTimer(); // POOLING THREAD 
            updateTeachers.Interval = TimeSpan.FromSeconds(5);
            updateTeachers.Tick += LoadTeachers;
            updateTeachers.Start();
        }


        private void LoadTeachers(object sender, EventArgs e)
        {
            driver.Service1Client srv = new driver.Service1Client();
            teachers = srv.GetAllTeacher().ToList();

            // Add computed RatingText for display
            foreach (var teacher in teachers)
            {
                teacher.RatingText = $"Rating: {String.Format("{0:0.00}", teacher.Rating)}/5";
            }

            TeacherListPanel.Items.Clear();
            foreach (var teacher in teachers)
            {
                var card = CreateTeacherCard(teacher);
                TeacherListPanel.Items.Add(card);
            }
        }
        private Border CreateTeacherCard(UserInfo teacher)
        {
            var chooseButton = new Button
            {
                Content = "Choose",
                Tag = teacher,
                Margin = new Thickness(6, 0, 0, 0),
                Padding = new Thickness(12, 6, 12, 6),
                Background = (Brush)new BrushConverter().ConvertFrom("#00E0FF"),
                Foreground = Brushes.Black,
                FontWeight = FontWeights.Bold,
                BorderBrush = (Brush)new BrushConverter().ConvertFrom("#00C0CC"),
                BorderThickness = new Thickness(1.5),
                Cursor = Cursors.Hand,
                Visibility = chooseMode ? Visibility.Visible : Visibility.Collapsed
            };
            chooseButton.Click += ChooseButton_Click;

            var reviewsButton = new Button
            {
                Content = "See Reviews",
                Tag = teacher,
                Margin = new Thickness(0, 0, 6, 0),
                Padding = new Thickness(12, 6, 12, 6),
                Background = (Brush)new BrushConverter().ConvertFrom("#00E0FF"),
                Foreground = Brushes.Black,
                FontWeight = FontWeights.Bold,
                BorderBrush = (Brush)new BrushConverter().ConvertFrom("#00C0CC"),
                BorderThickness = new Thickness(1.5),
                Cursor = Cursors.Hand
            };
            reviewsButton.Click += ReviewsButton_Click;

            var nameText = new TextBlock
            {
                Text = teacher.Username,
                FontSize = 22,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                Margin = new Thickness(0, 0, 0, 4)
            };

            var ratingText = new TextBlock
            {
                Text = teacher.RatingText,
                FontSize = 14,
                Foreground = Brushes.LightGray,
                Margin = new Thickness(0, 0, 0, 10)
            };

            var btnPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 10, 0, 0)
            };
            btnPanel.Children.Add(reviewsButton);
            btnPanel.Children.Add(chooseButton);

            var stack = new StackPanel();
            stack.Children.Add(nameText);
            stack.Children.Add(ratingText);
            stack.Children.Add(btnPanel);

            return new Border
            {
                Background = (Brush)new BrushConverter().ConvertFrom("#1A2E50"),
                BorderBrush = (Brush)new BrushConverter().ConvertFrom("#00E0FF"),
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(12),
                Margin = new Thickness(10),
                Padding = new Thickness(16),
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Colors.Black,
                    Direction = 270,
                    ShadowDepth = 4,
                    Opacity = 0.4,
                    BlurRadius = 8
                },
                Child = stack
            };
        }


        private void ChooseButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is UserInfo selectedTeacher)
            {
                MessageBox.Show($"You chose: {selectedTeacher.Username} (ID: {selectedTeacher.Id})");
                SignUp.Tid = selectedTeacher.Id;
                updateTeachers.Stop();
                page.Navigate(new SignUp(1));
            }
        }
        private List<UserInfo> GetAvailableTeachers()
        {
            driver.Service1Client srv = new driver.Service1Client();
            return srv.GetAllTeacher().ToList();
        }

        private void ReviewsButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is UserInfo selectedTeacher)
            {
                MessageBox.Show($"Viewing reviews for: {selectedTeacher.Username}");
                updateTeachers.Stop();
                page.Navigate(new Rewiews(selectedTeacher.Id, chooseMode));
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            page.Navigate(new StudentUI());
        }
    }
}
