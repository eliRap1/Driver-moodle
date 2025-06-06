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
    /// Interaction logic for AllStudents.xaml
    /// </summary>
    public partial class AllStudents : Page
    {
        private int teacherId;
        private DispatcherTimer updateStudents;
        public AllStudents(int teacherId)
        {
            InitializeComponent();
            this.teacherId = teacherId;
            LoadStudents(null, null);
            updateStudents = new DispatcherTimer(); // POOLING THREAD 
            updateStudents.Interval = TimeSpan.FromSeconds(5);
            updateStudents.Tick += LoadStudents;
            updateStudents.Start();
        }

        private void LoadStudents(object sender, EventArgs e)
        {
            driver.Service1Client srv = new driver.Service1Client();
            List<UserInfo> students = srv.GetTeacherStudents(this.teacherId).ToList();

            StudentListPanel.Items.Clear();
            foreach (var student in students)
            {
                StudentListPanel.Items.Add(CreateStudentCard(student));
            }
        }

        private Border CreateStudentCard(UserInfo student)
        {
            var stack = new StackPanel();

            stack.Children.Add(new TextBlock
            {
                Text = student.Username,
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White
            });

            stack.Children.Add(new TextBlock
            {
                Text = $"Email: {student.Email}",
                Margin = new Thickness(0, 4, 0, 2),
                Foreground = Brushes.LightGray
            });

            stack.Children.Add(new TextBlock
            {
                Text = $"Phone: {student.Phone}",
                Foreground = Brushes.LightGray,
                Margin = new Thickness(0, 0, 0, 8)
            });
            driver.Service1Client srv = new driver.Service1Client();
            if (student.Confirmed == false)
            {
                var confirmBtn = new Button
                {
                    Content = "Confirm",
                    Tag = student,
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00E0FF")),
                    Foreground = Brushes.Black,
                    BorderBrush = Brushes.Transparent,
                    FontWeight = FontWeights.Bold,
                    Width = 100,
                    Cursor = System.Windows.Input.Cursors.Hand
                };
                confirmBtn.Click += ConfirmBtn_Click;
                stack.Children.Add(confirmBtn);
            }
            else
            {
                stack.Children.Add(new TextBlock
                {
                    Text = "Confirmed",
                    Foreground = Brushes.LightGreen,
                    FontWeight = FontWeights.SemiBold
                });
            }

            return new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1A2E50")),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(15),
                Margin = new Thickness(10),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00E0FF")),
                BorderThickness = new Thickness(1),
                Child = stack
            };
        }

        private void ConfirmBtn_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is UserInfo student)
            {
                driver.Service1Client srv = new driver.Service1Client();
                srv.TeacherConfirm(student.Id, teacherId); // you should implement this method in your service
                MessageBox.Show($"{student.Username} confirmed!");
                LoadStudents(null, null); // Refresh list
            }
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            updateStudents.Stop();
            page.Navigate(new TeacherUI());
        }

    }
}
