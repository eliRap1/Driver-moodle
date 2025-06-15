using System;
using System.Collections.Generic;
using System.IO.Packaging;
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
    /// Interaction logic for ViewLessons.xaml
    /// </summary>
    public partial class ViewLessons : Page
    {
        public class Lesson
        {
            public string Date { get; set; }
            public string Time { get; set; }
        }
        public ViewLessons()
        {
            InitializeComponent();
            LoadLessons();
        }
        private void LoadLessons()
        {
            try
            {
                driver.Service1Client client = new driver.Service1Client();

                string dateAndTime = client.GetStudentLessons(LogIn.sign.Id);
                string[] dateAndTimeSplit = dateAndTime.Split(',');
                var upcomingLessons = new List<Lesson>();
                DateTime dateNow = DateTime.Now;
                int index = -1;
                for(int i = 0; i < dateAndTimeSplit.Length; i++)
                {
                    DateTime lessonDate = DateTime.Parse(dateAndTimeSplit[i]);
                    if (lessonDate < dateNow)
                    {
                        string date = dateAndTimeSplit[i].Split(' ')[0];
                        string time = dateAndTimeSplit[i].Split(' ')[1];
                        upcomingLessons.Add(new Lesson { Date = date, Time = time});
                    }
                    else
                    {
                        index = i;
                        break;
                    }
                
                }
                var completedLessons = new List<Lesson>();

                for (int i = index; i < dateAndTimeSplit.Length; i++)
                {
                    try
                    {
                        string date = dateAndTimeSplit[i].Split(' ')[0];
                        string time = dateAndTimeSplit[i].Split(' ')[1];
                        completedLessons.Add(new Lesson { Date = date, Time = time});
                    }
                    catch { }
                }

                UpcomingLessonsGrid.ItemsSource = completedLessons;
                CompletedLessonsGrid.ItemsSource = upcomingLessons;
            }
            catch { }
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            page.Navigate(new StudentUI());
        }
    }
}
