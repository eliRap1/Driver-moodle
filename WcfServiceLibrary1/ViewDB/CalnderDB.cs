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
        protected List<Calendars> SelectCal(string sqlCommandTxt)
        {
            List<Calendars> list = new List<Calendars>();
            try
            {
                connection.Open(); //was missing
                command.CommandText = sqlCommandTxt;
                reader = command.ExecuteReader();
                //NULLבנתיים לא בודקים האם אחד השדות הוא 
                while (reader.Read())
                {
                    Calendars entity = new Calendars(); //יוצר אובייקט מטיפוס המתאים
                    CreateModel(entity);
                    list.Add(entity);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message); //will word is every world, not only in world of Console

                //the output - we'll see in the output window of VisualStudio
            }
            finally
            {
                if (reader != null)
                    reader.Close();
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
            return list;
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
            List<Calendars> list = SelectCal(sqlStr);
            if (list.Count == 1)
            { return (Calendars)list[0]; }
            else { return null; }
        }
    }
}
