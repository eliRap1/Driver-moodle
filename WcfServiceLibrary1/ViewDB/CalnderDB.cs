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

                    // existing mapping for Availability table (if those columns exist)
                    if (HasColumn(reader, "UnavailableDate") && HasColumn(reader, "AllDay"))
                    {
                        // This row likely comes from TeacherUnavailableDate
                        // Build an UnavailableDay and add it to the Calendars.UnavailableDays list
                        var u = new UnavailableDay();
                        // some tables may store date as string or OleDb DateTime
                        DateTime dt;
                        if (DateTime.TryParse(reader["UnavailableDate"].ToString(), out dt))
                            u.Date = dt;
                        else
                            u.Date = DateTime.MinValue;

                        bool allday = false;
                        bool.TryParse(reader["AllDay"].ToString(), out allday);
                        u.AllDay = allday;

                        // StartTime / EndTime might be null in DB
                        u.StartTime = reader["StartTime"] != DBNull.Value ? reader["StartTime"].ToString() : null;
                        u.EndTime = reader["EndTime"] != DBNull.Value ? reader["EndTime"].ToString() : null;

                        if (s.UnavailableDays == null) s.UnavailableDays = new List<UnavailableDay>();
                        s.UnavailableDays.Add(u);
                    }
                    else
                    {
                        // Existing Availability mapping (Availability table)
                        s.DatesUnavailable = reader["UnavailableDate"].ToString().Split(',').ToList();
                        s.StartTime = reader["StartTime"].ToString();
                        s.EndTime = reader["EndTime"].ToString();
                        s.AvailableDays = reader["availableDays"].ToString().Split(',').ToList();
                        s.WorkingHours = reader["WorkingHours"].ToString().Split(',').ToList();
                        s.SelectedDay = reader["selectedDays"].ToString();
                        s.AllDay = bool.Parse(reader["AllDay"].ToString());
                        s.Teacherid = int.Parse(reader["TeacherID"].ToString());
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("CreateModel mapping issue: " + ex.Message);
                }
            }
        }
        private bool HasColumn(IDataRecord reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
        public Calendars GetTeacherCalendar(int teacherId)
        {
            string sqlStr = "Select * From Availability Where TeacherID=" + teacherId;
            List<Calendars> list = Select(sqlStr).OfType<Calendars>().ToList(); 
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

            try
            {
                // --- 1️⃣ Check if Availability row exists
                string existsSql = $"SELECT * FROM [Availability] WHERE [TeacherID] = {teacherId}";
                bool exists = Select(existsSql).OfType<Calendars>().Any();

                string startTime = cal.StartTime ?? "00:00";
                string endTime = cal.EndTime ?? "00:00";
                string availableDays = (cal.AvailableDays != null)
                    ? string.Join(",", cal.AvailableDays)
                    : "";

                // --- 2️⃣ Insert or Update Availability
                string sql;
                if (exists)
                {
                    sql = $@"
                UPDATE [Availability]
                SET [StartTime] = '{startTime}',
                    [EndTime]   = '{endTime}',
                    [availableDays] = '{availableDays}'
                WHERE [TeacherID] = {teacherId}";
                }
                else
                {
                    sql = $@"
                INSERT INTO [Availability] 
                    ([TeacherID],[StartTime],[EndTime],[availableDays])
                VALUES
                    ({teacherId},'{startTime}','{endTime}','{availableDays}')";
                }
                SaveChanges(sql);

                // --- 3️⃣ Update TeacherUnavailableDate table
                // delete previous entries (already present)
                string deleteUnavailable = $"DELETE FROM [TeacherUnavailableDate] WHERE [TeacherID] = {teacherId}";
                SaveChanges(deleteUnavailable);

                // insert new UnavailableDays rows (if present)
                if (cal.UnavailableDays != null && cal.UnavailableDays.Count > 0)
                {
                    foreach (var u in cal.UnavailableDays)
                    {
                        // if date is valid
                        if (u.Date != DateTime.MinValue)
                        {
                            // AllDay stored as 0/1 or True/False depending on Access schema (use True/False)
                            string allDayStr = u.AllDay ? "1" : "0";

                            // Protect nulls
                            string st = string.IsNullOrEmpty(u.StartTime) ? "" : u.StartTime;
                            string et = string.IsNullOrEmpty(u.EndTime) ? "" : u.EndTime;

                            string insertUnavailable = $@"
            INSERT INTO [TeacherUnavailableDate] 
                ([TeacherID],[UnavailableDate],[AllDay],[StartTime],[EndTime])
            VALUES ({teacherId}, '{u.Date:yyyy-MM-dd}', {allDayStr}, '{st}', '{et}')";
                            SaveChanges(insertUnavailable);
                        }
                    }
                }


                // --- 4️⃣ Update TeacherSpacialDays (clear + insert)
                string deleteSpecial = $"DELETE FROM [TeacherSpacialDays] WHERE [TeacherID] = {teacherId}";
                SaveChanges(deleteSpecial);

                if (cal.SpecialDaysList != null && cal.SpecialDaysList.ToArray().Length > 0)
                {
                    foreach (var sp in cal.SpecialDaysList)
                    {
                        string insertSpecial = $@"
                    INSERT INTO [TeacherSpacialDays] 
                        ([TeacherID],[SelectedDate],[StartTime],[EndTime])
                    VALUES 
                        ({teacherId}, '{sp.Date:yyyy-MM-dd}', '{sp.StartTime}', '{sp.EndTime}')";
                        SaveChanges(insertSpecial);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("SetTeacherCalendar failed: " + ex.Message);
                success = false;
            }

            return success;
        }


    }
}
