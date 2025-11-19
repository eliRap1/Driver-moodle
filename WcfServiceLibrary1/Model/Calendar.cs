using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Model
{
    public class Calendars : Base
    {
        private int teacherid;
        private string startTime;
        private string endTime;
        private List<string> datesUnavailable;
        private List<string> availableDays;
        private string unavailableDate;
        private string selectedDate;
        private string selectedDay;
        private List<string> workingHours;
        private bool allDay;

        // 🆕 Special working days (list of custom time slots)
        public List<SpecialDay> SpecialDaysList { get; set; } = new List<SpecialDay>();

        public string StartTime { get => startTime; set => startTime = value; }
        public string EndTime { get => endTime; set => endTime = value; }
        public List<string> DatesUnavailable { get => datesUnavailable; set => datesUnavailable = value; }
        public List<string> AvailableDays { get => availableDays; set => availableDays = value; }
        public string UnavailableDate { get => unavailableDate; set => unavailableDate = value; }
        public string SelectedDate { get => selectedDate; set => selectedDate = value; }
        public string SelectedDay { get => selectedDay; set => selectedDay = value; }

        // 🐞 Fixed recursive property
        public List<string> WorkingHours { get => workingHours; set => workingHours = value; }

        public bool AllDay { get => allDay; set => allDay = value; }
        public int Teacherid { get => teacherid; set => teacherid = value; }

        // --- Utility functions ---
        public string GetDatesUnavailable()
        {
            return datesUnavailable != null ? string.Join(",", datesUnavailable) : "";
        }

        public string GetAvailableDays()
        {
            return availableDays != null ? string.Join(",", availableDays) : "";
        }
    }

    // 🆕 New class: represents one special working day
    public class SpecialDay
    {
        public DateTime Date { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }

        public override string ToString()
        {
            return $"{Date:yyyy-MM-dd} ({StartTime}-{EndTime})";
        }
    }

    public class CalendarLst : List<Calendars>
    {
        public CalendarLst(IEnumerable<Base> list)
            : base(list.Cast<Calendars>().ToList()) { }
    }
}