using Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewDB
{
    public class CalnderDB : BaseDB
    {
        protected override Base NewEntity()
        {
            return new Calendars();
        }
        protected override void CreateModel(Base entity)
        {
            base.CreateModel(entity);
            if (entity != null)
            {
                try
                {
                    Calendars s = (Calendars)entity;
                    s.DatesUnavailable = reader["UnavailableDate"].ToString().Split(',').ToList();
                    s.StartTime = reader["StartTime"].ToString();
                    s.EndTime = reader["EndTime"].ToString();
                    s.AvailableDays = reader["availableDays"].ToString().Split(',').ToList();
                    s.WorkingHours = reader["WorkingHours"].ToString().Split(',').ToList();
                    s.SelectedDay = reader["selectedDays"].ToString();
                    s.AllDay = bool.Parse(reader["AllDay"].ToString());
                    s.Teacherid = int.Parse(reader["TeacherID"].ToString());

                }
                catch
                {
                    Console.WriteLine("No ID in DB");
                }
            }
        }
        public Calendars GetTeacherCalendar(int teacherId)
        {
            string sqlStr = "Select * From Availability Where TeacherID=" + teacherId;
            List<Calendars> list = Select(sqlStr).OfType<Calendars>().ToList(); ;
            if (list.Count == 1)
            { return (Calendars)list[0]; }
            else { return null; }
        }
        public List<Calendars> GetTeacherUnavailableDates(int teacherId)
        {
            string sqlStr = "SELECT * FROM TeacherUnavailableDate WHERE TeacherID = " + teacherId;
            List<Calendars> list = Select(sqlStr).OfType<Calendars>().ToList();
            return list;
        }

        public List<Calendars> TeacherSpacialDays(int teacherId)
        {
            string sqlStr = "SELECT * FROM TeacherSpacialDays WHERE TeacherID = " + teacherId;
            List<Calendars> list = Select(sqlStr).OfType<Calendars>().ToList();
            return list;
        }

    }
}
