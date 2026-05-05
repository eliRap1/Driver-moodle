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
        private int teacherId;


        public CalendarTeacher(Sign sign)
        {
            InitializeComponent();
            teacherId = ClientSession.TeacherId;
            LoadHourOptions();
            SpecialDaysList.ItemsSource = specialDays;
            UnavailableDaysList.ItemsSource = unavailableDays;
            LoadExistingAvailability();
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

        private void LoadExistingAvailability()
        {
            try
            {
                var cal = ServiceGateway.Use(client => client.GetTeacherCalendar(teacherId));
                if (cal == null)
                    return;

                SetDayChecks(cal.AvailableDays ?? new string[0]);
                SelectComboValue(StartHour, cal.StartTime);
                SelectComboValue(EndHour, cal.EndTime);

                if (cal.UnavailableDays != null)
                {
                    unavailableDays.Clear();
                    foreach (var day in cal.UnavailableDays)
                        unavailableDays.Add(day);
                }

                if (cal.SpecialDaysList != null)
                {
                    specialDays.Clear();
                    foreach (var day in cal.SpecialDaysList)
                        specialDays.Add(day);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadExistingAvailability Error: {ex.Message}");
            }
        }

        private void SelectComboValue(ComboBox combo, string value)
        {
            if (string.IsNullOrEmpty(value))
                return;

            for (int i = 0; i < combo.Items.Count; i++)
            {
                if (combo.Items[i].ToString() == value)
                {
                    combo.SelectedIndex = i;
                    return;
                }
            }
        }

        private void SetDayChecks(string[] days)
        {
            var selected = new HashSet<string>(days);
            MondayCheck.IsChecked = selected.Contains("Monday");
            TuesdayCheck.IsChecked = selected.Contains("Tuesday");
            WednesdayCheck.IsChecked = selected.Contains("Wednesday");
            ThursdayCheck.IsChecked = selected.Contains("Thursday");
            FridayCheck.IsChecked = selected.Contains("Friday");
            SaturdayCheck.IsChecked = selected.Contains("Saturday");
            SundayCheck.IsChecked = selected.Contains("Sunday");
        }
        private void AddUnavailableDay_Click(object sender, RoutedEventArgs e)
        {
            if (UnavailableDatePicker.SelectedDate == null) return;

            DateTime date = UnavailableDatePicker.SelectedDate.Value;
            bool allDay = UnavailableAllDay.IsChecked == true;
            string start = allDay ? null : (UnavailableStartHour.SelectedItem?.ToString() ?? "00:00");
            string end = allDay ? null : (UnavailableEndHour.SelectedItem?.ToString() ?? "00:00");

            if (!allDay && string.Compare(start, end, StringComparison.Ordinal) >= 0)
            {
                MessageBox.Show("Blocked time must end after it starts.");
                return;
            }

            if (unavailableDays.Any(day => day.Date.Date == date.Date && day.AllDay == allDay && day.StartTime == start && day.EndTime == end))
                return;

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
            if (string.Compare(SpecialStartHour.SelectedItem.ToString(), SpecialEndHour.SelectedItem.ToString(), StringComparison.Ordinal) >= 0)
            {
                MessageBox.Show("Special day must end after it starts.");
                return;
            }
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
                if (ServiceGateway.Use(client => client.SetTeacherCalendar(cal, teacherId)))
                    MessageBox.Show("Availability saved successfully!");
                else
                    MessageBox.Show("Failed to save availability.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
    }
}
