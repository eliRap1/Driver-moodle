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

namespace driver_client
{
    /// <summary>
    /// Interaction logic for Teacher_Schedule.xaml
    /// </summary>
    public partial class Teacher_Schedule : Page
    {
        // simple model for lessons
        public class Lesson
        {
            public string StudentID { get; set; }
            public string LessonTime { get; set; }
            public int LessonId { get; set; }
            public string Date { get; set; }
            public string Time { get; set; }
            public string Status { get; set; }
        }

        public Teacher_Schedule()
        {
            InitializeComponent();
            LessonDatePicker.SelectedDate = DateTime.Now; // set today as default
        }

        private void ShowLessons_Click(object sender, RoutedEventArgs e)
        {
            if (LessonDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Please select a date.");
                return;
            }

            DateTime selectedDate = LessonDatePicker.SelectedDate.Value;
            string dateString = selectedDate.ToString("dd-MM-yyyy");
            //List<Lessons> lessons = GetLessonsForDate(dateString);
            List<Lessons> allLessons = GetAllLessons(selectedDate);
            var historyLessons = new List<Lesson>();
            var dateLessons = new List<Lesson>();

            foreach (var lesson in allLessons)
            {
                if (DateTime.TryParse($"{lesson.Date} {lesson.Time}", out DateTime lessonDateTime))
                {
                    var item = new Lesson
                    {
                        LessonId = lesson.LessonId,
                        Date = lessonDateTime.ToString("dd/MM/yyyy"),
                        Time = lessonDateTime.ToString("HH:mm"),
                        Status = lesson.paid ? "Yes" : "No"
                    };

                    if (lessonDateTime.Date == selectedDate.Date)
                        dateLessons.Add(item);
                    historyLessons.Add(item);
                }
            }
            DayLessons.ItemsSource = dateLessons;
            HistoryLessons.ItemsSource = historyLessons;

        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            
        }

        // mock data for now
        //private List<Lessons> GetLessonsForDate(string date)
        //{
        //    driver.Service1Client srv = new driver.Service1Client();
        //    List<Lessons> sample = srv.GetAllTeacherLessonsForDate(LogIn.sign.Id, date).ToList<Lessons>();
        //    return sample;
        //}

        private List<Lessons> GetAllLessons(DateTime date)
        {
            driver.Service1Client srv = new driver.Service1Client();
            List<Lessons> sample = srv.GetAllTeacherLessons(LogIn.sign.Id).ToList<Lessons>();

            return sample;
        }
    }
}