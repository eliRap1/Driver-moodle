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
        public string StartTime { get => startTime; set => startTime = value; }
        public string EndTime { get => endTime; set => endTime = value; }
        public List<string> DatesUnavailable { get => datesUnavailable; set => datesUnavailable = value; }
        public List<string> AvailableDays { get => availableDays; set => availableDays = value; }
        public string UnavailableDate { get => unavailableDate; set => unavailableDate = value; }
        public string SelectedDate { get => selectedDate; set => selectedDate = value; }
        public string SelectedDay { get => selectedDay; set => selectedDay = value; }
        public List<string> WorkingHours { get => WorkingHours; set => WorkingHours = value; }
        public bool AllDay { get => allDay; set => allDay = value; }
        public int Teacherid { get => teacherid; set => teacherid = value; }



        public string GetDatesUnavailable()
        {
            string result = "";
            foreach (string date in datesUnavailable)
            {
                result += date + ",";
            }
            return result;
        }
        public string GetAvailableDays()
        {
            string result = "";
            foreach (string day in availableDays)
            {
                result += day + ",";
            }
            return result;
        }
    }
    public class CalendarLst : List<Calendars>
    {
        public CalendarLst(IEnumerable<Base> List) : base(List.Cast<Calendars>().ToList()) { }
    }
}
