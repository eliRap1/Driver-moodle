using Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
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
        public bool SetTeacherCalendar(Calendars cal, int teacherId)
        {
            bool success = true;
            connection.Open();
            OleDbTransaction transaction = connection.BeginTransaction();

            try
            {
                // 1️⃣ Check if Availability row exists
                string existsSql = "SELECT COUNT(*) FROM [Availability] WHERE [TeacherID] = " + teacherId;
                int count;
                using (var cmd = new OleDbCommand(existsSql, connection, transaction))
                {
                    count = (int)cmd.ExecuteScalar();
                }

                // 2️⃣ Update or insert Availability
                string sql;
                if (count > 0)
                {
                    sql = $@"
                UPDATE [Availability]
                   SET [UnavailableDate] = '{cal.GetDatesUnavailable()}',
                       [StartTime]       = '{cal.StartTime}',
                       [EndTime]         = '{cal.EndTime}',
                       [availableDays]   = '{cal.GetAvailableDays()}'
                 WHERE [TeacherID] = {teacherId}";
                }
                else
                {
                    sql = $@"
                INSERT INTO [Availability]
                    ([TeacherID],[UnavailableDate],[StartTime],[EndTime],[availableDays])
                VALUES
                    ({teacherId},'{cal.GetDatesUnavailable()}','{cal.StartTime}','{cal.EndTime}','{cal.GetAvailableDays()}')";
                }

                using (var cmd = new OleDbCommand(sql, connection, transaction))
                {
                    cmd.ExecuteNonQuery();
                }

                // 3️⃣ Update TeacherSpacialDays table (clear + reinsert)
                string deleteSpecial = $"DELETE FROM TeacherSpacialDays WHERE TeacherID = {teacherId}";
                using (var cmd = new OleDbCommand(deleteSpecial, connection, transaction))
                {
                    cmd.ExecuteNonQuery();
                }

                if (cal.SpecialDaysList != null && cal.SpecialDaysList.Count > 0)
                {
                    foreach (var sp in cal.SpecialDaysList)
                    {
                        string insertSpecial = $@"
                    INSERT INTO TeacherSpacialDays (TeacherID, SpecialDate, StartTime, EndTime)
                    VALUES ({teacherId}, '{sp.Date:yyyy-MM-dd}', '{sp.StartTime}', '{sp.EndTime}')";
                        using (var cmd = new OleDbCommand(insertSpecial, connection, transaction))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                transaction.Commit();
            }
            catch (Exception ex)
            {
                Console.WriteLine("SetTeacherCalendar failed: " + ex.Message);
                transaction.Rollback();
                success = false;
            }
            finally
            {
                connection.Close();
            }

            return success;
        }

    }
}
