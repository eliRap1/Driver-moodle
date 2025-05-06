using System;
using System.Collections.Generic;
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
            var availableDays = new List<string>();
            if (MondayCheck.IsChecked == true) availableDays.Add("Monday");
            if (TuesdayCheck.IsChecked == true) availableDays.Add("Tuesday");
            if (WednesdayCheck.IsChecked == true) availableDays.Add("Wednesday");
            if (ThursdayCheck.IsChecked == true) availableDays.Add("Thursday");
            if (FridayCheck.IsChecked == true) availableDays.Add("Friday");
            if (SaturdayCheck.IsChecked == true) availableDays.Add("Saturday");
            if (SundayCheck.IsChecked == true) availableDays.Add("Sunday");

            string fromTime = StartHour.SelectedItem?.ToString() ?? "N/A";
            string toTime = EndHour.SelectedItem?.ToString() ?? "N/A";

            var unavailableDates = UnavailableCalendar.SelectedDates;

            // You can save these details to a database or display a confirmation
            MessageBox.Show(
                $"Available Days: {string.Join(", ", availableDays)}\n" +
                $"Working Hours: {fromTime} to {toTime}\n" +
                $"Unavailable Dates: {string.Join(", ", unavailableDates)}",
                "Availability Saved");
        }

        private void ClearSelection_Click(object sender, RoutedEventArgs e)
        {
            UnavailableCalendar.SelectedDates.Clear();
        }
    }
}
