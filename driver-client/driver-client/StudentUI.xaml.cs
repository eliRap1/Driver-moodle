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
    /// Interaction logic for StudentUI.xaml
    /// </summary>
    public partial class StudentUI : Page
    {
        private int id;
        private DispatcherTimer updateAprove;
        public static bool madeRewiew = false;
        public StudentUI()
        {
            InitializeComponent();
            driver.Service1Client srv = new driver.Service1Client();
            id = srv.GetUserID(LogIn.sign.Username, "Student");
            updateAprove = new DispatcherTimer(); // POOLING THREAD 
            updateAprove.Interval = TimeSpan.FromSeconds(5);
            updateAprove.Tick += CheckIfApproved;
            updateAprove.Start();
            CheckIfApproved(null, null);
            if(madeRewiew)
            {
                writeReview.IsEnabled = false;
            }
        }

        private void CheckIfApproved(object sender, EventArgs e)
        {
            driver.Service1Client srv = new driver.Service1Client();
            var student = srv.GetUserById(id, "Student");
            if (student.Confirmed == true)
            {
                WaitingPanel.Visibility = Visibility.Collapsed;
                StudentPanel.Visibility = Visibility.Visible;
                updateAprove.Stop();
            }
            else
            {
                WaitingPanel.Visibility = Visibility.Visible;
                StudentPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void ScheduleLesson_Click(object sender, RoutedEventArgs e)
        {
            page.Navigate(new ScheduleLesson());
        }

        private void WriteReview_Click(object sender, RoutedEventArgs e)
        {
            page.Navigate(new WriteRewiew());
        }

        private void ViewLessons_Click(object sender, RoutedEventArgs e)
        {
            page.Navigate(new ViewLessons());
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void Review_Click(object sender, RoutedEventArgs e)
        {
            page.Navigate(new ChooseTeacher(false));
        }
        private void Chat_Click(object sender, RoutedEventArgs e)
        {
            page.Navigate(new Chat());
        }
    }

}
