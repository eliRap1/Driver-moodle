using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class Calendars : Base
    {
        private string startTime;
        private string endTime;
        private List<string> datesUnavailable;
        private List<string> availableDays;
        public string StartTime { get => startTime; set => startTime = value; }
        public string EndTime { get => endTime; set => endTime = value; }
        public List<string> DatesUnavailable { get => datesUnavailable; set => datesUnavailable = value; }
        public List<string> AvailableDays { get => availableDays; set => availableDays = value; }
        public Calendars() { }

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
}
