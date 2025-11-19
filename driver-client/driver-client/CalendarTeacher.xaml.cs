using driver_client.driver;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace driver_client
{
    public partial class CalendarTeacher : Page
    {
        private ObservableCollection<SpecialDay> specialDays = new ObservableCollection<SpecialDay>();

        public CalendarTeacher(Sign sign)
        {
            InitializeComponent();
            LoadHourOptions();
            SpecialDaysList.ItemsSource = specialDays;
        }

        private void LoadHourOptions()
        {
            for (int hour = 6; hour <= 22; hour++)
            {
                string time = $"{hour:00}:00";
                StartHour.Items.Add(time);
                EndHour.Items.Add(time);
                SpecialStartHour.Items.Add(time);
                SpecialEndHour.Items.Add(time);
            }
            StartHour.SelectedIndex = 0;
            EndHour.SelectedIndex = 1;
            SpecialStartHour.SelectedIndex = 0;
            SpecialEndHour.SelectedIndex = 1;
        }

        private void AddSpecialDay_Click(object sender, RoutedEventArgs e)
        {
            if (SpecialDatePicker.SelectedDate == null) return;
            if(SpecialStartHour.SelectedItem == null || SpecialEndHour.SelectedItem == null) return;
            if(specialDays.Any(d => d.Date == SpecialDatePicker.SelectedDate.Value)) return;
            var day = new SpecialDay
            {
                Date = SpecialDatePicker.SelectedDate.Value,
                StartTime = SpecialStartHour.SelectedItem?.ToString() ?? "00:00",
                EndTime = SpecialEndHour.SelectedItem?.ToString() ?? "00:00"
            };

            specialDays.Add(day);
        }

        private void RemoveSpecialDay_Click(object sender, RoutedEventArgs e)
        {
            if (SpecialDaysList.SelectedItem is SpecialDay selected)
                specialDays.Remove(selected);
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            page.Navigate(new TeacherUI());
        }
        private void ClearSelection_Click(object sender, RoutedEventArgs e)
        {
            specialDays.Clear();
        }
        private void BackToAvailabillity_Click(object sender, RoutedEventArgs e)
        {
            SpecialDaysGroupBox.Visibility = Visibility.Collapsed;  //need to update
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

            string fromTime = StartHour.SelectedItem?.ToString() ?? "00:00";
            string toTime = EndHour.SelectedItem?.ToString() ?? "00:00";

            var unavailableDateStrings = UnavailableCalendar.SelectedDates
                .Select(d => d.ToString("yyyy-MM-dd")).ToList();

            try
            {
                driver.Service1Client srv = new driver.Service1Client();
                int teacherId = LogIn.sign.Id;
                Calendars cal = new Calendars
                {
                    StartTime = fromTime,
                    EndTime = toTime,
                    DatesUnavailable = unavailableDateStrings.ToArray(),
                    AvailableDays = availableDays.ToArray(),
                    SpecialDaysList = specialDays.ToArray()
                };

                srv.SetTeacherCalendar(cal, teacherId);
                MessageBox.Show("Availability saved successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void SpacialDays_Click(object sender, RoutedEventArgs e)
        {
            SpecialDaysGroupBox.Visibility = Visibility.Visible;
        }
    }
}
