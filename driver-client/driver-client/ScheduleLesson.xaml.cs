using driver_client.driver;
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
    /// Interaction logic for ScheduleLesson.xaml
    /// </summary>
    public partial class ScheduleLesson : Page
    {
        private List<string> unavailableDates;
        private List<string> availableDays;
        private DateTime startTime;
        private DateTime endTime;

        public ScheduleLesson()
        {
            InitializeComponent();

            driver.Service1Client srv = new driver.Service1Client();

            int id = srv.GetTeacherId(LogIn.sign.Id); 
            Calendars cal = srv.GetTeacherCalendar(id);
            List<Lessons> lessons = srv.GetAllStudentLessons(LogIn.sign.Id).ToList();
            if (cal != null)
            {
                unavailableDates = cal.DatesUnavailable != null ? cal.DatesUnavailable.ToList() : new List<string>();
                availableDays = cal.AvailableDays != null ? cal.AvailableDays.ToList() : new List<string>();
                startTime = DateTime.Parse(cal.StartTime);
                endTime = DateTime.Parse(cal.EndTime);
            }
            else
            {
                unavailableDates = new List<string>();
                availableDays = new List<string>();
    
            }
            while (endTime >= startTime)
            {
                if (HasTime(startTime, lessons))
                {
                    //lessonTimeComboBox.Items.Add("Unavailable");
                    startTime = startTime.AddHours(1);
                    continue;
                }
                lessonTimeComboBox.Items.Add(startTime.ToString("HH:mm"));
                startTime = startTime.AddHours(1);
            }


            lessonDatePicker.DisplayDateStart = DateTime.Today;
            lessonDatePicker.BlackoutDates.Clear();
            lessonDatePicker.SelectedDateChanged += LessonDatePicker_SelectedDateChanged;

            DisableUnavailableDates();
        }
        private bool HasTime(DateTime startTime, List<Lessons> lessons)
        {
            foreach (var lesson in lessons)
            {
                if (DateTime.TryParse($"{lesson.Date} {lesson.Time}", out DateTime lessonDateTime))
                {
                    if (lessonDateTime.Hour == startTime.Hour && lessonDateTime.Minute == startTime.Minute)
                        return true;
                }
            }
            return false;
        }
        private void DisableUnavailableDates()
        {
            foreach (var dateStr in unavailableDates)
            {
                if (DateTime.TryParse(dateStr, out DateTime date))
                {
                    lessonDatePicker.BlackoutDates.Add(new CalendarDateRange(date));
                }
            }

            lessonDatePicker.DateValidationError += (s, e) =>
            {
                e.ThrowException = false;
                MessageBox.Show("Selected date is not available.");
            };
        }

        private void LessonDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lessonDatePicker.SelectedDate is DateTime selectedDate)
            {
                string selectedDay = selectedDate.DayOfWeek.ToString();

                if (!availableDays.Contains(selectedDay))
                {
                    MessageBox.Show($"{selectedDay} is not an available day for lessons.");
                    lessonDatePicker.SelectedDate = null;
                }
            }
        }

        private void ConfirmLesson_Click(object sender, RoutedEventArgs e)
        {
            if (lessonDatePicker.SelectedDate == null || lessonTimeComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select both a date and a time.");
                return;
            }

            string selectedDate = lessonDatePicker.SelectedDate.Value.ToShortDateString();
            string selectedTime = lessonTimeComboBox.SelectedItem.ToString();
            driver.Service1Client srv = new driver.Service1Client();
            srv.AddLessonForStudent(LogIn.sign.Id, selectedDate, selectedTime);
            MessageBox.Show($"Lesson scheduled for {selectedDate} at {selectedTime}.");
            page.Navigate(new StudentUI());
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            page.Navigate(new StudentUI());
        }
    }

}
