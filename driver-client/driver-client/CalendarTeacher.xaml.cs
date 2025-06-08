using driver_client.driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace driver_client
{
    public partial class CalendarTeacher : Page
    {
        public CalendarTeacher(Sign sign)
        {
            InitializeComponent();
            LoadHourOptions();
        }

        private void LoadHourOptions()
        {
            for (int hour = 6; hour <= 22; hour++)
            {
                string time = $"{hour:00}:00";
                StartHour.Items.Add(time);
                EndHour.Items.Add(time);
            }
            StartHour.SelectedIndex = 0;
            EndHour.SelectedIndex = 1;
        }

        private void SaveAvailability_Click(object sender, RoutedEventArgs e)
        {
            // Collect selected day names
            var availableDays = new List<string>();
            if (MondayCheck.IsChecked == true) availableDays.Add("Monday");
            if (TuesdayCheck.IsChecked == true) availableDays.Add("Tuesday");
            if (WednesdayCheck.IsChecked == true) availableDays.Add("Wednesday");
            if (ThursdayCheck.IsChecked == true) availableDays.Add("Thursday");
            if (FridayCheck.IsChecked == true) availableDays.Add("Friday");
            if (SaturdayCheck.IsChecked == true) availableDays.Add("Saturday");
            if (SundayCheck.IsChecked == true) availableDays.Add("Sunday");

            string fromTime = StartHour.SelectedItem?.ToString() ?? "00:00";
            string toTime = EndHour.SelectedItem?.ToString() ?? "00:00";

            // Convert selected dates to string format
            List<string> unavailableDateStrings = UnavailableCalendar.SelectedDates
                .Select(date => date.ToString("yyyy-MM-dd"))
                .ToList();

            try
            {
                driver.Service1Client srv = new driver.Service1Client();
                int teacherId = LogIn.sign.Id;
                Calendars cal = new Calendars();
                cal.StartTime = fromTime;
                cal.EndTime = toTime;
                cal.DatesUnavailable = unavailableDateStrings.ToArray();
                cal.AvailableDays = availableDays.ToArray();
                srv.SetTeacherCalendar(cal, teacherId);
                MessageBox.Show("Availability saved successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }


        private void ClearSelection_Click(object sender, RoutedEventArgs e)
        {
            UnavailableCalendar.SelectedDates.Clear();
        }
        private void UnavailableCalendar_PreviewMouseDown(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is FrameworkElement fe && fe.DataContext is DateTime clickedDate)
            {
                if (UnavailableCalendar.SelectedDates.Contains(clickedDate))
                    UnavailableCalendar.SelectedDates.Remove(clickedDate);
                else
                    UnavailableCalendar.SelectedDates.Add(clickedDate);

                e.Handled = true; // Prevent default selection behavior
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            page.Navigate(new TeacherUI());
        }

    }
}
