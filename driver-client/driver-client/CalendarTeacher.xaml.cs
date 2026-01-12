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
        private ObservableCollection<UnavailableDay> unavailableDays = new ObservableCollection<UnavailableDay>();


        public CalendarTeacher(Sign sign)
        {
            InitializeComponent();
            LoadHourOptions();
            SpecialDaysList.ItemsSource = specialDays;
            UnavailableDaysList.ItemsSource = unavailableDays;
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

                UnavailableStartHour.Items.Add(time);
                UnavailableEndHour.Items.Add(time);
            }
            StartHour.SelectedIndex = 0;
            EndHour.SelectedIndex = 1;
            SpecialStartHour.SelectedIndex = 0;
            SpecialEndHour.SelectedIndex = 1;

            UnavailableStartHour.SelectedIndex = 0;
            UnavailableEndHour.SelectedIndex = 1;
        }
        private void AddUnavailableDay_Click(object sender, RoutedEventArgs e)
        {
            if (UnavailableDatePicker.SelectedDate == null) return;

            DateTime date = UnavailableDatePicker.SelectedDate.Value;
            bool allDay = UnavailableAllDay.IsChecked == true;
            string start = allDay ? null : (UnavailableStartHour.SelectedItem?.ToString() ?? "00:00");
            string end = allDay ? null : (UnavailableEndHour.SelectedItem?.ToString() ?? "00:00");

            // prevent duplicates for same date
            if (unavailableDays.Any(day => day.Date.Date == date.Date)) return;

            var u = new UnavailableDay
            {
                Date = date,
                AllDay = allDay,
                StartTime = start,
                EndTime = end
            };

            unavailableDays.Add(u);
        }

        private void RemoveUnavailableDay_Click(object sender, RoutedEventArgs e)
        {
            if (UnavailableDaysList.SelectedItem is UnavailableDay selected)
                unavailableDays.Remove(selected);
        }


        private void AddSpecialDay_Click(object sender, RoutedEventArgs e)
        {
            if (SpecialDatePicker.SelectedDate == null) return;
            if (SpecialStartHour.SelectedItem == null || SpecialEndHour.SelectedItem == null) return;
            if (specialDays.Any(d => d.Date == SpecialDatePicker.SelectedDate.Value)) return;
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
            UnavailableDaysGroupBox.Visibility = Visibility.Collapsed;
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
                
            var cal = new Calendars
            {
                StartTime = fromTime,
                EndTime = toTime,
                DatesUnavailable = unavailableDays.Select(u => u.Date.ToString("yyyy-MM-dd")).ToArray(),
                AvailableDays = availableDays.ToArray(),
                SpecialDaysList = specialDays.ToArray()
            };

            // attach rich unavailable days
            cal.UnavailableDays = unavailableDays.ToArray();

            try
            {
                driver.Service1Client srv = new driver.Service1Client();
                int teacherId = LogIn.sign.Id;
                if (srv.SetTeacherCalendar(cal, teacherId))
                    MessageBox.Show("Availability saved successfully!");
                else
                    MessageBox.Show("Failed to save availability.");
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

        private void UnavailableDays_Click(object sender, RoutedEventArgs e)
        {
            UnavailableDaysGroupBox.Visibility = Visibility.Visible;
        }
    }
}
