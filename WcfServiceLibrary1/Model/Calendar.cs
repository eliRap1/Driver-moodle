using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class Calendar : Base
    {
        private string name;
        private DateTime startTime;
        private DateTime endTime;
        private List<DateTime> datesUnavailable;
        public string Name { get => name; set => name = value; }
        public DateTime StartTime { get => startTime; set => startTime = value; }
        public DateTime EndTime { get => endTime; set => endTime = value; }
        public List<DateTime> DatesUnavailable { get => datesUnavailable; set => datesUnavailable = value; }
        
        public Calendar() { }

        public Calendar(string name, DateTime startDate, DateTime endDate,  List<DateTime> datesUnavailable)
        {
            this.name = name;
            this.startTime = startDate;
            this.endTime = endDate;
            this.DatesUnavailable = datesUnavailable;
        }
        public string UnavailableDates()
        {
            string unavailableDates = "";
            foreach (DateTime date in DatesUnavailable)
            {
                unavailableDates += date.ToString() + "%";
            }
            return unavailableDates;
        }
    }
}
