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
    }
}
